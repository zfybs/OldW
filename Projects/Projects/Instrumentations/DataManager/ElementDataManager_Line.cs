using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Autodesk.Revit.DB;
using OldW.GlobalSettings;
using OldW.Instrumentations;
using stdOldW;
using stdOldW.WinFormHelper;
using Form = System.Windows.Forms.Form;


namespace OldW.DataManager
{
    public partial class ElementDataManager
    {
        internal class DgvLine
        {
            #region    ---   Types

            /// <summary>
            /// 与一个线测点相关的所有数据，包括测点信息，所有的监测数据，以及对应表格实体类的信息
            /// </summary>
            private class TableBindedData
            {
                #region    ---   属性

                /// <summary> 线测点对象 </summary>
                public readonly Instrum_Line Monitor;

                /// <summary> 此线测点的实体类所对应的程序集 </summary>
                public Assembly Assembly;

                /// <summary>
                /// 在DataGridView中记录的监测数据，这些数据并不一定完全符合监测数据实体类的数据格式，
                /// 只是存储下来以供后面再次切换到此测点时可以直接将数据恢复到Datagridview中，
                /// 而不用每一次都去构造对应的实体类并且为每一天的实例数据赋值。
                /// </summary>
                public BindingList<object> DatagridViewData;

                /// <summary>
                /// 此测点的节点数据。如果在DataGridView中修改了一个测点的节点信息，则其最新的节点数据会体现在这里。
                /// </summary>
                public string[] Nodes;

                /// <summary> Datagridview 所绑定 表示线测点监测数据的实体类的名称，其所在的命名空间保存在常数 <see cref="NamespaceName"/> 中</summary>
                public string EntityName;

                #endregion

                /// <summary>
                /// 构造函数
                /// </summary>
                /// <param name="monitor"></param>
                /// <param name="assemblyData"></param>
                /// <param name="entityName"> 实体类的名称 </param>
                public TableBindedData(Instrum_Line monitor, Assembly assemblyData, string entityName)
                {
                    DatagridViewData = new BindingList<object>();
                    this.Monitor = monitor;

                    if (monitor.GetMonitorData() != null)
                    {
                        Nodes = monitor.GetMonitorData().GetStringNodes();
                    }
                    else
                    {
                        Nodes = new string[0];
                    }
                    this.Assembly = assemblyData;
                    this.EntityName = entityName;
                }

                #region    ---   方法

                /// <summary>
                /// 返回此测点所对应的实体类
                /// </summary>
                /// <returns></returns>
                public Type GetEntityClass()
                {
                    return Assembly.GetType(NamespaceName + "." + EntityName);
                }

                /// <summary>
                /// 返回指定数据行的监测数据
                /// </summary>
                /// <param name="rowIndexes"> 要提取的数据在BindingList中的行号
                /// （正常情况下其值应该与Datagridview中的行号相同，但是如果出现不同，以Bindinglist为准） </param>
                /// <param name="date"> 指定行中所对应的时间数据 </param>
                /// <returns> 指定行中所对应的监测数据 </returns>
                public float?[][] GetMonitorData(int[] rowIndexes, out DateTime[] date)
                {
                    if (DatagridViewData == null || DatagridViewData.Count == 0)
                    {
                        throw new NullReferenceException();
                    }

                    int daysCount = rowIndexes.Length;
                    Type tp = GetEntityClass();
                    PropertyInfo[] props = tp.GetProperties();

                    //
                    float?[][] AllData = new float?[daysCount][];
                    date = new DateTime[daysCount];
                    int row;
                    for (int day = 0; day < daysCount; day++)
                    {
                        row = rowIndexes[day];
                        object value = DatagridViewData[row]; // 一天的所有数据

                        // 提取日期数据
                        date[day] = (DateTime)props[0].GetValue(value);

                        // 提取监测数据
                        float?[] oneDayData = new float?[Nodes.Length];
                        for (int i = 1; i <= Nodes.Length; i++)
                        { // 提取每一个属性中对应的值，第一个属性为日期，不进行提取
                            oneDayData[i - 1] = (float?)props[i].GetValue(value);
                        }
                        AllData[day] = oneDayData;
                    }
                    return AllData;
                }

                /// <summary>
                /// 生成一个实体类的实例对象
                /// </summary>
                /// <returns></returns>
                public object CreateInstance()
                {
                    return Assembly.CreateInstance(NamespaceName + "." + EntityName);
                }
                #endregion
            }

            #endregion

            #region    ---   Fields

            private readonly Document doc;

            private readonly eZDataGridViewPaste dataGridViewLine;

            /// <summary>
            /// 已经编译好的实体类对象
            /// </summary>
            static SortedDictionary<int, Assembly> CompiledAssemlies = new SortedDictionary<int, Assembly>();

            /// <summary> 表格中当前处理的那个测点。在为此属性赋值的过程中，会执行相关的刷新操作 </summary>
            private ElementId activeInstruId;

            /// <summary>
            /// 当前以经打开并且提取过监测数据的线测点对象。
            /// </summary>
            private readonly Dictionary<ElementId, TableBindedData> OpenedTableSet;

            #endregion

            #region    ---   构造函数 与 控件初始化

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="doc"></param>
            /// <param name="dataGridViewLine"></param>
            public DgvLine(Document doc, eZDataGridViewPaste dataGridViewLine)
            {
                this.dataGridViewLine = dataGridViewLine;
                this.doc = doc;
                this.OpenedTableSet = new Dictionary<ElementId, TableBindedData>();

                //
                ConstructDataGridView();

                // 事件绑定
                dataGridViewLine.DataError += new DataGridViewDataErrorEventHandler(this.MyDataGridView1_DataError);
            }

            /// <summary>
            /// 创建 DataGridView 为点测点监测数据类型
            /// </summary>
            private void ConstructDataGridView()
            {
                //-------------------- 设置数据源的集合 -------------------------------------


                //-------------------- 设置 dataGridView -------------------------------------

                // 如果AutoGenerateColumns 为False，那么当设置DataSource 后，用户必须要手动为指定的属性值添加数据列。
                dataGridViewLine.AutoGenerateColumns = false;
                dataGridViewLine.AutoSize = false;
                dataGridViewLine.AllowUserToAddRows = true;

                // 添加日期列
                DataGridViewColumn column;
                column = new DataGridViewTextBoxColumn();
                column.DataPropertyName = "Date";
                // 此列所对应的数据源中的元素中的哪一个属性的名称
                column.HeaderText = @"日期";
                column.Name = @"日期";
                column.ValueType = typeof(DateTime);
                dataGridViewLine.Columns.Add(column);

                // 数据绑定
                BindingList<object> bindedTableData = new BindingList<object> { AllowNew = true };

                dataGridViewLine.DataSource = bindedTableData;
                bindedTableData.AddingNew += bindedTableData_AddingNew; // 事件关联
            }


            #endregion

            #region    ---   DataGridView中的数据与监测参数值之间的交互

            /// <summary> 与表格数据进行绑定的点测点监测数据集合 </summary>
            /// <summary>
            /// 切换到新的线监测点
            /// </summary>
            public void ShiftToNewElement(Instrum_Line ele)
            {
                // ------------------------ 刷新到新测点 ----------------------------


                ElementId eid = ele.Monitor.Id;
                TableBindedData tbd;
                if (OpenedTableSet.ContainsKey(eid))
                {
                    tbd = OpenedTableSet[eid];

                    ChangeColumns(tbd.Nodes.Length, tbd.Nodes);

                    // 填充 Datagridview （直接用上次打开时的数据）
                    FillBindingListFromList(tbd.DatagridViewData, tbd);
                    // 
                    activeInstruId = eid;
                }
                else // 说明此单元还从未打开过
                {
                    tbd = InitialMonitorLine(ele);

                    OpenedTableSet.Add(eid, tbd);
                    // 
                    activeInstruId = eid;
                }
            }

            /// <summary>
            /// 设置当前活动节点的数据
            /// </summary>
            public void ChangeNodes()
            {
                if (activeInstruId == null)
                {
                    return;
                }

                TableBindedData tbd = OpenedTableSet[activeInstruId];
                if (tbd.Monitor.NodesDigital)
                {
                    var nodes = tbd.Nodes;
                    var fn = new FormNodes(nodes);

                    // 打开窗口并操作
                    fn.ShowDialog();

                    RefreshNodes(fn.Nodes);
                }
            }

            /// <summary>
            /// 将表格中的数据保存到Element的对应参数中。
            /// </summary>
            /// <remarks></remarks>
            public void SaveTableToElement()
            {
                // 将表格数据构造为监测数据类，并将其保存到测点对象中
                using (Transaction tran = new Transaction(doc, "保存表格中的数据到Element的参数中"))
                {
                    tran.Start();
                    try
                    {
                        TableBindedData tdb = OpenedTableSet[activeInstruId];

                        var monitorValue = ConvertBindingList(tdb);

                        // 
                        var monitorDataLine = new MonitorData_Line(tdb.Nodes, monitorValue, OpenedTableSet[activeInstruId].Monitor.NodesDigital);

                        OpenedTableSet[activeInstruId].Monitor.SetMonitorData(tran, monitorDataLine);
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        Utils.ShowDebugCatch(ex, "无法保存监测数据到对象参数中。");
                        tran.RollBack();
                    }
                }
            }

            #endregion

            #region    ---   线测点节点信息修改

            /// <summary>
            /// 对于还没有打开过的element，将其在 Datagridview 中打开，同时初始化相应的 TableBindedData 数据。
            /// </summary>
            /// <param name="ele"></param>
            private TableBindedData InitialMonitorLine(Instrum_Line ele)
            {
                if (OpenedTableSet.ContainsKey(ele.Monitor.Id))
                { throw new InvalidOperationException("此测点已经打开过了，不需要再进行初始化。"); }

                // 提取监测数据
                MonitorData_Line ML = ele.GetMonitorData();
                string[] newNodes;
                if (ML == null)
                {
                    // 说明Element的参数中没有包含有效的监测数据
                    newNodes = new string[0];
                }
                else
                {
                    // 说明Element的参数中包含有有效的监测数据
                    newNodes = ML.GetStringNodes();
                }

                // 根据节点数目返回对应的程序集
                string entityName;
                Assembly assemblyData = GetEntityAssembly(newNodes.Length, out entityName);

                // 构造数据
                TableBindedData tbd = new TableBindedData(ele, assemblyData, entityName);

                // 填充 Datagridview 
                FillBindingListFromParameter(tbd);

                return tbd;
            }

            /// <summary>
            /// 为指定的单元设置新的节点信息，并同时刷新 Datagridview 控件中的数据
            /// </summary>
            /// <param name="ele"></param>
            /// <param name="newNodes"> 节点修改前后，重新绑定数据集合与界面的刷新 </param>
            /// <remarks> 当节点信息改变后，有以下几种情况：
            ///  1、 如果节点数目没有变，则 DataSource 不用变，只需要修改表格的表头文字而已
            ///  2、 如果节点数目发生了变化，则 DataSource 所绑定的集合中的泛型将要发生改变，此时需要重新构造BindingList集合，并且将原来的数据行转换到新的集合中去。</remarks>
            private void RefreshNodes(string[] newNodes)
            {
                if (activeInstruId != null && OpenedTableSet.ContainsKey(activeInstruId))
                {
                    TableBindedData tbd = OpenedTableSet[activeInstruId];

                    // 刷新DatagridView的界面：数据列的增删以及列头标题的修改
                    ChangeColumns(newNodes.Length, newNodes);

                    // 当节点信息改变后，有以下几种情况
                    // 1、 如果节点数目没有变，则 DataSource 不用变，只需要修改表格的表头文字而已
                    // 2、 如果节点数目发生了变化，则 DataSource 所绑定的集合中的泛型将要发生改变，此时需要重新构造BindingList集合，并且将原来的数据行转换到新的集合中去。

                    // 根据节点数目返回对应的程序集
                    string entityName;
                    Assembly assemblyData = GetEntityAssembly(newNodes.Length, out entityName);

                    // 重新绑定数据集合
                    BindingList<object> newDataSource = GetNewConstructedDataSource(tbd, assemblyData, entityName);
                    FillBindingListFromList(newDataSource, tbd);

                    // 将新的数据存储起来（必须要执行）
                    tbd.Assembly = assemblyData;
                    tbd.EntityName = entityName;
                    tbd.DatagridViewData = newDataSource;
                    tbd.Nodes = newNodes;
                }
            }


            #endregion

            #region    ---   监测数据 BindingList 的相关操作

            /// <summary> 将 DataGridView 控件中绑定的数据集合转换为实体类 <see cref="MonitorData_Point"/> 的集合。 </summary>
            /// <remarks> BindingList 中的元素如果有测点的监测日期为null，则进行剔除。</remarks>
            private SortedDictionary<DateTime, float?[]> ConvertBindingList(TableBindedData bindedAssembly)
            {
                SortedDictionary<DateTime, float?[]> data = new SortedDictionary<DateTime, float?[]>();
                Type tp = bindedAssembly.GetEntityClass();
                PropertyInfo[] props = tp.GetProperties();

                //
                BindingList<object> monitorDataSet = (BindingList<object>)dataGridViewLine.DataSource;
                object dtt;
                DateTime dt;
                foreach (var oneDayValue in monitorDataSet)
                {
                    dtt = props[0].GetValue(oneDayValue);
                    if (dtt != null)
                    {
                        // 提取日期数据
                        dt = (DateTime)dtt;

                        float?[] oneDay = new float?[props.Length - 1];
                        for (int i = 1; i < props.Length; i++)
                        {
                            oneDay[i - 1] = (float?)props[i].GetValue(oneDayValue);
                        }
                        data.Add(dt, oneDay);
                    }
                }
                return data;
            }

            /// <summary> 将指定单元中的监测数据参数中的监测数据提取出来并 填充到 BindingList 集合中。</summary>
            /// <param name="monitorDataSet"> 某线测点的监测数据 </param>
            private void FillBindingListFromParameter(TableBindedData monitorDataSet)
            {
                BindingList<object> bindedList = new BindingList<object>() { AllowNew = true };
                // bindedList.Clear();

                Type tp = monitorDataSet.GetEntityClass();
                // 根据实体类型构造表格
                PropertyInfo[] props = tp.GetProperties(); // 类型的所有属性，其中第一个为日期，后面的为每一个子节点
                ChangeColumns(props.Length - 1, monitorDataSet.Nodes);

                // 提取监测数据
                DateTime dt;
                object EntityInstance;
                MonitorData_Line ml = monitorDataSet.Monitor.GetMonitorData() ?? new MonitorData_Line(new string[0], monitorDataSet.Monitor.NodesDigital);
                // 将监测数据中的值更新到 Datagridview 绑定的集合中
                foreach (var oneDayValues in ml.MonitorData)
                {
                    // 设置日期属性
                    EntityInstance = monitorDataSet.CreateInstance();
                    dt = oneDayValues.Key;
                    props[0].SetValue(EntityInstance, dt);

                    // 为每一个子节点属性设置对应的监测数据值
                    var nodes = oneDayValues.Value;
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        props[i + 1].SetValue(EntityInstance, nodes[i]);
                    }

                    // 将一条设置好的记录添加到集合中
                    bindedList.Add(EntityInstance);
                }
                // 将得到的最新数据保存下来
                monitorDataSet.DatagridViewData = bindedList;

                // 刷新DataGridView中的数据集合
                FillBindingListFromList(bindedList, monitorDataSet);
            }

            /// <summary> 将Datagridview的DataSource进行重新绑定，并刷新事件AddingNew事件关联。</summary>
            /// <param name="sourceData"> 用来填充 Datagridview 的数据集合  </param>
            private void FillBindingListFromList(BindingList<object> sourceData, TableBindedData tbd)
            {
                // 取消上一个绑定集合的事件关联
                BindingList<object> bindedData = (BindingList<object>)dataGridViewLine.DataSource;
                bindedData.AddingNew -= bindedTableData_AddingNew;

                // 绑定新的集合
                if (sourceData.Count == 0)
                {
                    /*  只有在对DataSource属性进行赋值、或者是修改DataGridViewColumn. DataPropertyName时，才会改变表格中数据列的绑定关系。
                     *  除此之外，对于在BindingList< Object>集合中还没有元素的情况下赋值给DataSource的情况，
                     *  DataGridView会根据在BindingList.Add()方法（而不是AddNew方法或者AddingNew事件）中添加的第一个元素值（不能是null）的具体类型还创建数据列的绑定关系。
                     */
                    sourceData.Add(tbd.CreateInstance());
                }

                dataGridViewLine.DataSource = sourceData;
                // 重新关联新的事件
                sourceData.AddingNew += bindedTableData_AddingNew;
            }

            #endregion

            #region    ---   实体类的动态构造 与 Datagridview 列的修改

            /// <summary> Datagridview 所绑定 表示线测点监测数据的实体类所在的命名空间的名称</summary>
            private const string NamespaceName = "OldW.LineMonitorEntity";

            /// <summary> 返回指定节点数据所对应的实体类 </summary>
            /// <param name="nodesCount"> 如果nodesCount的值为0，则返回的实体类中只有一个属性“Date”</param>
            /// <param name="entityName"> 编译的实体类的名称，即“Nodes_3” </param>
            private Assembly GetEntityAssembly(int nodesCount, out string entityName)
            {
                // 在类名中注释此类有多少个节点
                entityName = "Nodes" + @"_" + nodesCount; // DateTime.Now.ToString("yyMMddhhmmss");
                // 在编译时已经将编译成的dll的名称与对应的entityName相同。
                string dllPath = Path.Combine(ProjectPath.Path_DataGridViewLineMonitorEntityClass, entityName + ".dll");

                if (CompiledAssemlies.ContainsKey(nodesCount))
                {
                    return CompiledAssemlies[nodesCount];
                }
                else if (File.Exists(dllPath))
                {
                    // 检查是否有已经编译好的dll可以直接加载的
                    Assembly ass = Assembly.LoadFile(dllPath);
                    CompiledAssemlies.Add(nodesCount, ass);
                    return ass;
                }
                else
                {
                    // 动态编译对应的实体类
                    AssemblyCompiler assemblyData = CompileEntity(entityName, nodesCount);
                    Assembly ass = assemblyData.CompilerResults.CompiledAssembly;
                    //
                    CompiledAssemlies.Add(nodesCount, ass);
                    return ass;
                }
            }

            /// <summary>
            /// 修改表格的列数。表格中的第一列始终是日期列，这一列是永远不会被删除的。
            /// </summary>
            /// <param name="nodesCount"> 新表格中有多少个节点 </param>
            /// <param name="nodes"> 可选参数，表示每一个节点的位置，用来显示在表格的表头处。 </param>
            private void ChangeColumns(int nodesCount, string[] nodes = null)
            {
                int lastColumnNum = dataGridViewLine.ColumnCount - 1;
                // 表格中最后一列所对应的节点编号，如果LastColumnNum的值为0，则表示只有第一列即日期列

                int added = nodesCount - lastColumnNum; // 要添加的列数

                DataGridViewColumn column;
                if (added == 0)
                {
                    // 很好，不作任何变化  
                }
                else if (added < 0)
                {
                    // 删除几行，注意下标编号
                    for (int i = lastColumnNum; i > nodesCount; i--)
                    {
                        dataGridViewLine.Columns.RemoveAt(i);
                    }
                }
                else if (added > 0)
                {
                    // 添加几行
                    for (int nodeNum = lastColumnNum + 1; nodeNum <= nodesCount; nodeNum++)
                    {
                        column = new DataGridViewTextBoxColumn();
                        column.DataPropertyName = "Node" + nodeNum;
                        column.Name = "Node" + nodeNum;
                        column.HeaderText = @"节点" + nodeNum;
                        column.ValueType = typeof(float?);
                        dataGridViewLine.Columns.Add(column);
                    }
                }

                // 修改列的字段名称
                if (nodes != null && nodes.Length == nodesCount)
                {
                    for (int nodeNum = 1; nodeNum <= nodesCount; nodeNum++)
                    {
                        dataGridViewLine.Columns[nodeNum].HeaderText = string.Format("节点{0}({1})", nodeNum,
                            nodes[nodeNum - 1]);
                    }
                }
            }

            /// <summary> 编译对应的实体类的Dll </summary>
            /// <param name="EntityName">实体类的名称。此实体类会被放置在OldW.LineMonitorEntity命名空间下。
            /// 对应的dll文件的名称为“EntityName + ".dll" ”。</param>
            /// <param name="nodesCount">此类中的属性个数，即线测点中的节点个数。</param>
            /// <returns></returns>
            private AssemblyCompiler CompileEntity(string EntityName, int nodesCount)
            {
                CodeCompileUnit compileUnit = ConstructEntityByNodes(EntityName, nodesCount);

                string[] refDlls = new string[] { "system.dll" };

                string dllName = Path.Combine(ProjectPath.Path_DataGridViewLineMonitorEntityClass, EntityName + @".dll");

                AssemblyCompiler acp = new AssemblyCompiler(compileUnit, refDlls);
                acp.CompileAssembly(dllName);

                return acp;
            }

            /// <summary>
            /// 示例代码：生成CodeDOM图，这一步是最复杂的部分，后面生成代码与编译都是以这里的东西为蓝本
            /// </summary>
            /// <returns>此函数仅为示例，并不在此类中执行。外部代码可以参数本函数来创建出对应的 CodeCompileUnit 源代码结构 </returns>
            private static CodeCompileUnit ConstructEntityByNodes(string entityName, int nodesCount)
            {
                //生成一个可编译的单元，这是最根部的东西
                CodeCompileUnit compunit = new CodeCompileUnit();
                CodeNamespace sampleNamespace = new CodeNamespace(NamespaceName); //定义一个名为Sample的命名空间

                CodeTypeDeclaration MyClass = new CodeTypeDeclaration(entityName); //定义一个名为DemoClass的类

                CodeMemberField myField;
                CodeMemberProperty MyProperty;

                // 添加第一列的日期字段

                // 添加字段
                myField = new CodeMemberField("System.Nullable<System.DateTime>", "_date");
                myField.Attributes = MemberAttributes.Private;
                MyClass.Members.Add(myField);

                // 添加属性
                MyProperty = new CodeMemberProperty();
                MyProperty.Name = "Date";
                MyProperty.Type = new CodeTypeReference("System.Nullable<System.DateTime>");
                MyProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                MyProperty.GetStatements.Add(
                    new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
                        "_date")));
                MyProperty.SetStatements.Add(
                    new CodeAssignStatement(
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_date"),
                        new CodePropertySetValueReferenceExpression()));
                MyClass.Members.Add(MyProperty);

                // 将线测点中每一个子节点创建出对应的属性
                string fieldName;
                string propertyName;

                for (int i = 1; i <= nodesCount; i++)
                {
                    fieldName = "_node" + i;
                    propertyName = "Node" + i;

                    // 添加字段
                    myField = new CodeMemberField("System.Nullable<System.Single>", fieldName);
                    myField.Attributes = MemberAttributes.Private;
                    MyClass.Members.Add(myField);

                    // 添加属性
                    MyProperty = new CodeMemberProperty();
                    MyProperty.Name = propertyName;
                    MyProperty.Type = new CodeTypeReference("System.Nullable<System.Single>");
                    MyProperty.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                    MyProperty.GetStatements.Add(
                        new CodeMethodReturnStatement(new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(),
                            fieldName)));
                    MyProperty.SetStatements.Add(
                        new CodeAssignStatement(
                            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName),
                            new CodePropertySetValueReferenceExpression()));
                    MyClass.Members.Add(MyProperty);
                }

                //下面一系列语句把上述定义好的元素联接起来
                compunit.Namespaces.Add(sampleNamespace);
                sampleNamespace.Imports.Add(new CodeNamespaceImport("System")); //导入System命名空间
                sampleNamespace.Types.Add(MyClass);

                return compunit;
            }

            private BindingList<object> GetNewConstructedDataSource(TableBindedData sourceObject, Assembly desAssembly, string desEntityName)
            {
                Type type1 = sourceObject.Assembly.GetType(NamespaceName + "." + sourceObject.EntityName);
                Type type2 = desAssembly.GetType(NamespaceName + "." + desEntityName);
                //
                PropertyInfo[] props1 = type1.GetProperties();
                PropertyInfo[] props2 = type2.GetProperties();
                //
                if (props1.Length == props2.Length)
                {
                    // 很好，不用作任何改变
                    return sourceObject.DatagridViewData;
                }
                else
                {
                    // 说明要增加或者减少节点并重新赋值 
                    BindingList<object> newData = new BindingList<object>() { AllowNew = true };
                    object ins2;
                    foreach (object ins1 in sourceObject.DatagridViewData)
                    {
                        ins2 = desAssembly.CreateInstance(type2.FullName);

                        // 将源对象中的属性值赋值给新对象中的对应属性
                        // 在属性赋值时保证相同属性合并的原则，哪一个节点少就以哪一个为准。
                        for (int i = 0; i < Math.Min(props1.Length, props2.Length); i++)
                        {
                            props2[i].SetValue(ins2, props1[i].GetValue(ins1));
                        }
                        newData.Add(ins2);
                    }
                    return newData;
                }
            }

            #endregion

            #region    ---   绘制监测曲线图

            /// <summary>
            /// 绘制图表
            /// </summary>
            /// <param name="data"></param>
            public Chart_MonitorData DrawData()
            {
                if (activeInstruId != null)
                {
                    //  根据不同的节点数的线测点类型，创建不同的实体类的初始值。

                    TableBindedData monitorDataSet = OpenedTableSet[activeInstruId];

                    int daysCount = dataGridViewLine.SelectedRows.Count;
                    if (daysCount == 0)
                    {
                        MessageBox.Show("请先选择至少一行监测数据", "提示");
                        return null;
                    }
                    int[] rowIndexes = new int[daysCount];
                    for (int i = 0; i < daysCount; i++)
                    {
                        rowIndexes[i] = dataGridViewLine.SelectedRows[i].Index;
                    }

                    // 所有指定的日期的监测数据
                    DateTime[] allDate;
                    float?[][] allData = monitorDataSet.GetMonitorData(rowIndexes, out allDate);

                    // 绘图
                    Chart_MonitorData Chart1 = new Chart_MonitorData(monitorDataSet.Monitor.Type);
                    // 设置图例
                    Chart1.Chart.Legends.Clear();
                    Chart1.Chart.Legends.Add(new Legend("图例"));

                    // 添加监测曲线
                    Series s;
                    for (int i = 0; i < daysCount; i++)
                    {
                        s = Chart1.AddLineSeries(allDate[i].ToShortDateString() + allDate[i].ToShortTimeString());  // $"系列{i}"
                        s.Points.DataBindXY(monitorDataSet.Nodes, allData[i]);
                    }

                    Chart1.Show();
                    return Chart1;
                }
                return null;
            }

            #endregion

            #region    ---   事件处理

            /// <summary>
            /// 表格中输入的数据不能进行正常转换时的异常处理
            /// </summary>
            public void MyDataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
            {
                if ((e.Context & DataGridViewDataErrorContexts.Parsing) != 0)
                {
                    Type tp = dataGridViewLine.Columns[e.ColumnIndex].ValueType;
                    MessageBox.Show("输入的数据不能转换为指定类型的数据: \n\r " + tp.Name,
                        "数据格式转换出错。", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.ThrowException = false;
                }
            }

            public void bindedTableData_AddingNew(object sender, AddingNewEventArgs e)
            {
                if (activeInstruId != null)
                {
                    //  根据不同的节点数的线测点类型，创建不同的实体类的初始值。

                    TableBindedData monitorDataSet = OpenedTableSet[activeInstruId];
                    object entityInstance = monitorDataSet.CreateInstance();

                    e.NewObject = entityInstance;
                }
            }

            #endregion
        }

        #region    ---   设置节点信息的窗口

        /// <summary>
        /// 打开窗口以设置最新的节点数据。
        /// </summary>
        private class FormNodes : Form
        {
            private eZDataGridViewPaste _dataGridView;

            // 设置好的节点数据
            private string[] _nodes;

            public string[] Nodes
            {
                get { return _nodes; }
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="Nodes"></param>
            public FormNodes(string[] Nodes)
            {
                Initialize();

                if (Nodes.Length > 0)
                {
                    _dataGridView.Rows.Add(Nodes.Length);
                    for (int i = 0; i < Nodes.Length; i++)
                    {
                        _dataGridView.Rows[i].Cells[0].Value = Nodes[i];
                    }
                }

                // 添加事件关联
                _dataGridView.DataError += _dataGridView_DataError;
                FormClosed += new FormClosedEventHandler(Form_Closed);
            }

            /// <summary>
            /// 界面初始化
            /// </summary>
            private void Initialize()
            {
                // 窗体UI
                StartPosition = FormStartPosition.CenterScreen;
                Width = 250;
                Text = @"节点位置";
                MaximizeBox = false;
                MinimizeBox = false;

                // 添加控件与数据绑定
                _dataGridView = new eZDataGridViewPaste();

                // Initialize and add a text box column.
                // 先创建一个列，然后将列中的数据与数据源集合中的某个属性相关联即可。
                DataGridViewColumn column = new DataGridViewTextBoxColumn();
                column.ValueType = typeof(float);
                column.HeaderText = @"节点";

                //
                _dataGridView.Columns.Add(column);

                _dataGridView.Dock = DockStyle.Fill;
                _dataGridView.AllowUserToAddRows = true;
                _dataGridView.AutoSize = false;

                //
                Controls.Add(_dataGridView);
            }

            /// <summary>
            /// 表格中输入的数据不能进行正常转换时的异常处理
            /// </summary>
            private void _dataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
            {
                if ((e.Context & DataGridViewDataErrorContexts.Parsing) != 0)
                {
                    e.ThrowException = false;
                }
            }

            private void Form_Closed(object sender, FormClosedEventArgs e)
            {
                try
                {
                    // 提取数据
                    int nodesCount = _dataGridView.Rows.Count - 1;
                    object v;
                    _nodes = new string[nodesCount];
                    for (int i = 0; i < nodesCount; i++)
                    {
                        v = _dataGridView.Rows[i].Cells[0].Value;

                        if (v != null)
                        {
                            _nodes[i] = (string)v;
                        }
                    }
                }
                finally
                {
                    Dispose();
                }
            }
        }

        #endregion
    }
}
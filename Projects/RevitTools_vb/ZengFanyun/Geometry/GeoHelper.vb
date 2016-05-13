Imports System
Imports System.Collections.Generic
Imports Autodesk.Revit
Imports Autodesk.Revit.DB
Imports System.Math

Namespace rvtTools_ez
    ''' <summary>
    ''' A object to help locating with geometry data.
    ''' </summary>
    Public Class GeoHelper

        ''' <summary>
        ''' 进行点的距离比较时的容差。
        ''' Revit中，Application.VertexTolerance属性值返回的值为：0.0005233832795，
        ''' 也就是说，如果两个点的距离小于这个值，就认为这两个点是重合的。
        ''' </summary>
        Public Const VertexTolerance As Double = 0.0005

        ''' <summary>
        ''' Find the bottom face of a face array.
        ''' </summary>
        ''' <param name="faces">A face array.</param>
        ''' <returns>The bottom face of a face array.</returns>
        Public Shared Function GetBottomFace(ByVal faces As FaceArray) As Face
            Dim face As Face = Nothing
            Dim elevation As Double = 0
            Dim tempElevation As Double = 0
            Dim mesh As Mesh = Nothing

            For Each f As Face In faces
                If IsVerticalFace(f) Then
                    ' If this is a vertical face, it cannot be a bottom face to a certainty.
                    Continue For
                End If

                tempElevation = 0
                mesh = f.Triangulate()

                For Each xyz As Autodesk.Revit.DB.XYZ In mesh.Vertices
                    tempElevation = tempElevation + xyz.Z
                Next xyz

                tempElevation = tempElevation / mesh.Vertices.Count

                If elevation > tempElevation OrElse Nothing Is face Then
                    ' Update the bottom face to which's elevation is the lowest.
                    face = f
                    elevation = tempElevation
                End If
            Next f

            ' The bottom face is consider as which's average elevation is the lowest, except vertical face.
            Return face
        End Function

        ''' <summary>
        ''' Find out the three points which made of a plane.
        ''' </summary>
        ''' <param name="mesh">A mesh contains many points.</param>
        ''' <param name="startPoint">Create a new instance of ReferencePlane.</param>
        ''' <param name="endPoint">The free end apply to reference plane.</param>
        ''' <param name="thirdPnt">A third point needed to define the reference plane.</param>
        Public Shared Sub Distribute(ByVal mesh As Mesh, ByRef startPoint As Autodesk.Revit.DB.XYZ, ByRef endPoint As Autodesk.Revit.DB.XYZ, ByRef thirdPnt As Autodesk.Revit.DB.XYZ)
            Dim count As Integer = mesh.Vertices.Count
            startPoint = mesh.Vertices(0)
            endPoint = mesh.Vertices(CInt(count \ 3))
            thirdPnt = mesh.Vertices(CInt(count \ 3 * 2))
        End Sub

        ''' <summary>
        ''' Calculate the length between two points.
        ''' </summary>
        ''' <param name="startPoint">The start point.</param>
        ''' <param name="endPoint">The end point.</param>
        ''' <returns>The length between two points.</returns>
        Public Shared Function GetLength(ByVal startPoint As Autodesk.Revit.DB.XYZ, ByVal endPoint As Autodesk.Revit.DB.XYZ) As Double
            Return Math.Sqrt(Math.Pow((endPoint.X - startPoint.X), 2) + Math.Pow((endPoint.Y - startPoint.Y), 2) + Math.Pow((endPoint.Z - startPoint.Z), 2))
        End Function

        ''' <summary>
        ''' The distance between two value in a same axis.
        ''' </summary>
        ''' <param name="start">start value.</param>
        ''' <param name="end">end value.</param>
        ''' <returns>The distance between two value.</returns>
        Public Shared Function GetDistance(ByVal start As Double, ByVal [end] As Double) As Double
            Return Math.Abs(start - [end])
        End Function

        ''' <summary>
        ''' Get the vector between two points.
        ''' </summary>
        ''' <param name="startPoint">The start point.</param>
        ''' <param name="endPoint">The end point.</param>
        ''' <returns>The vector between two points.</returns>
        Public Shared Function GetVector(ByVal startPoint As Autodesk.Revit.DB.XYZ, ByVal endPoint As Autodesk.Revit.DB.XYZ) As Autodesk.Revit.DB.XYZ
            Return New Autodesk.Revit.DB.XYZ(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y, endPoint.Z - startPoint.Z)
        End Function

        ''' <summary>
        ''' Determines whether a face is vertical.
        ''' </summary>
        ''' <param name="face">The face to be determined.</param>
        ''' <returns>Return true if this face is vertical, or else return false.</returns>
        Private Shared Function IsVerticalFace(ByVal face As Face) As Boolean
            For Each ea As EdgeArray In face.EdgeLoops
                For Each e As Edge In ea
                    If IsVerticalEdge(e) Then
                        Return True
                    End If
                Next e
            Next ea

            Return False
        End Function

        ''' <summary>
        ''' Determines whether a edge is vertical.
        ''' </summary>
        ''' <param name="edge">The edge to be determined.</param>
        ''' <returns>Return true if this edge is vertical, or else return false.</returns>
        Private Shared Function IsVerticalEdge(ByVal edge As Edge) As Boolean
            Dim polyline As List(Of XYZ) = TryCast(edge.Tessellate(), List(Of XYZ))
            Dim verticalVct As New Autodesk.Revit.DB.XYZ(0, 0, 1)
            Dim pointBuffer As Autodesk.Revit.DB.XYZ = polyline(0)

            For i As Integer = 1 To polyline.Count - 1
                Dim temp As Autodesk.Revit.DB.XYZ = polyline(i)
                Dim vector As Autodesk.Revit.DB.XYZ = GetVector(pointBuffer, temp)
                If Equal(vector, verticalVct) Then
                    Return True
                Else
                    Continue For
                End If
            Next i

            Return False
        End Function

        ''' <summary>
        ''' Determines whether two vector are equal in x and y axis.
        ''' </summary>
        ''' <param name="vectorA">The vector A.</param>
        ''' <param name="vectorB">The vector B.</param>
        ''' <returns>Return true if two vector are equals, or else return false.</returns>
        Private Shared Function Equal(ByVal vectorA As Autodesk.Revit.DB.XYZ, ByVal vectorB As Autodesk.Revit.DB.XYZ) As Boolean
            Dim isNotEqual As Boolean = (VertexTolerance < Math.Abs(vectorA.X - vectorB.X)) OrElse (VertexTolerance < Math.Abs(vectorA.Y - vectorB.Y))
            Return If(isNotEqual, False, True)
        End Function

        ''' <summary> 比较两个点之间的距离是否小于指定的容差 </summary>
        ''' <remarks>对于Revit中的XYZ对象，其也有一个IsAlmostEqualTo函数，但是要注意，
        ''' 那个函数是用来比较两个向量的方向是否小于指定的弧度容差。</remarks>
        Public Shared Function IsAlmostEqualTo(Point1 As XYZ, Point2 As XYZ, ByVal Precision As Double) As Boolean
            Dim D As Double = Sqrt((Point1.X - Point2.X) ^ 2 +
                                   (Point1.Y - Point2.Y) ^ 2 +
                                   (Point1.Z - Point2.Z) ^ 2)
            If D <= Precision Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' 找到Extrusion中指定法向的平面。如果有多个平面的法向都是指定的法向，则返回第一个找到的平面。
        ''' Given a solid, find a planar face with the given normal (version 2)
        ''' this is a slightly enhanced version which checks if the face is on the given reference plane.
        ''' </summary>
        ''' <param name="refPlane">除了验证平面的法向外，还可以额外验证一下指定法向的平面是否是在指定的参考平面上。即要同时满足normal与ReferencePlane两个条件。
        ''' additionally, we want to check if the face is on the reference plane</param>
        ''' <remarks></remarks>
        Public Shared Function FindFace(ByVal aSolid As Extrusion, ByVal normal As XYZ, Optional ByVal refPlane As ReferencePlane = Nothing) As PlanarFace

            '' get the geometry object of the given element
            ''
            Dim op As New Options
            op.ComputeReferences = True
            ' Dim geomObjs As GeometryObjectArray = aSolid.Geometry(op).Objects

            '' loop through the array and find a face with the given normal
            ''
            For Each geomObj As GeometryObject In aSolid.Geometry(op)

                If TypeOf geomObj Is Solid Then  ''  solid is what we are interested in.

                    Dim pSolid As Solid = geomObj
                    Dim faces As FaceArray = pSolid.Faces

                    For Each pFace As Face In faces
                        If TypeOf pFace Is PlanarFace Then
                            Dim pPlanarFace As PlanarFace = pFace
                            If Not (pPlanarFace Is Nothing) Then
                                ''  check to see if they have same normal
                                If pPlanarFace.ComputeNormal(New UV(0, 0)).IsAlmostEqualTo(normal) Then

                                    If refPlane Is Nothing Then
                                        Return pPlanarFace  '' we found the face. 
                                    Else
                                        ''  additionally, we want to check if the face is on the reference plane
                                        ''  get a point on the face. Any point will do.
                                        Dim pEdge As Edge = pPlanarFace.EdgeLoops.Item(0).Item(0)
                                        Dim pt As XYZ = pEdge.Evaluate(0.0)
                                        ''  is the point on the reference plane? 
                                        Dim res As Boolean = IsPointOnPlane(pt, refPlane.GetPlane)
                                        If res Then
                                            Return pPlanarFace  '' we found the face 
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Next

                ElseIf TypeOf geomObj Is GeometryInstance Then
                    '' will come back later as needed.

                ElseIf TypeOf geomObj Is Curve Then
                    '' will come nack later as needed.

                ElseIf TypeOf geomObj Is Mesh Then
                    '' will come back later as needed.

                Else
                    '' what else do we have?

                End If
            Next

            '' if we come here, we did not find any.
            Return Nothing

        End Function

        ''' <summary>
        ''' 判断一个三维点是否在指定的平面上
        ''' </summary>
        ''' <param name="p1"></param>
        ''' <param name="plane"></param>
        ''' <returns></returns>
        Public Shared Function IsPointOnPlane(ByVal p1 As XYZ, ByVal plane As Plane)

            Dim dt As Double = (plane.Normal).DotProduct(p1 - (plane.Origin)) ' 指定点到原点法向向量的投影长度，即此点到对应平面距离
            If dt < VertexTolerance Then ' 说明此点到指定平面的距离很小
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' 判断一个三维点是否在指定的参考平面上
        ''' </summary>
        ''' <param name="p1"></param>
        ''' <param name="plane">参考平面，ReferencePlane.GetPlane方法也可以返回Plane对象。</param>
        ''' <returns></returns>
        Public Shared Function IsPointOnPlane(ByVal p1 As XYZ, ByVal plane As ReferencePlane)

            Dim dt As Double = (plane.Normal).DotProduct(p1 - (plane.BubbleEnd)) ' 指定点到原点法向向量的投影长度，即此点到对应平面距离
            If dt < VertexTolerance Then ' 说明此点到指定平面的距离很小
                Return True
            Else
                Return False
            End If
        End Function

    End Class

End Namespace
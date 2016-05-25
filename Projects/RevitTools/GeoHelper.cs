// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Data;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using System.Collections;
using System.Linq;
// End of VB project level imports

using Autodesk.Revit;
using Autodesk.Revit.DB;
//using System.Math;


namespace rvtTools
{
	/// <summary>
	/// A object to help locating with geometry data.
	/// </summary>
	public class GeoHelper
	{
		
		/// <summary>
		/// 进行点的距离比较时的容差。
		/// Revit中，Application.VertexTolerance 属性值返回的值为：0.0005233832795，
		/// 也就是说，如果两个点的距离小于这个值，就认为这两个点是重合的。
		/// </summary>
		public const double VertexTolerance = 0.0005;
		
		/// <summary>
		/// 进行点的距离比较时的容差。
		/// Revit中，Application.AngleTolerance 属性值返回的值为：0.00174532925199433，
		/// 也就是说，如果两个角度的区别小于这个值，就认为这两个角度是相同的的。
		/// Two angle measurements closer than this value are considered identical.
		/// </summary>
		public const double AngleTolerance = 0.0015;
		
		/// <summary>
		/// Find the bottom face of a face array.
		/// </summary>
		/// <param name="faces">A face array.</param>
		/// <returns>The bottom face of a face array.</returns>
		public static Face GetBottomFace(FaceArray faces)
		{
			Face face = null;
			double elevation = 0;
			double tempElevation = 0;
			Mesh mesh = null;
			
			foreach (Face f in faces)
			{
				if (IsVerticalFace(f))
				{
					// If this is a vertical face, it cannot be a bottom face to a certainty.
					continue;
				}
				
				tempElevation = 0;
				mesh = f.Triangulate();
				
				foreach (Autodesk.Revit.DB.XYZ xyz in mesh.Vertices)
				{
					tempElevation = tempElevation + xyz.Z;
				}
				
				tempElevation = tempElevation / mesh.Vertices.Count;
				
				if (elevation > tempElevation || null == face)
				{
					// Update the bottom face to which's elevation is the lowest.
					face = f;
					elevation = tempElevation;
				}
			}
			
			// The bottom face is consider as which's average elevation is the lowest, except vertical face.
			return face;
		}
		
		/// <summary>
		/// Find out the three points which made of a plane.
		/// </summary>
		/// <param name="mesh">A mesh contains many points.</param>
		/// <param name="startPoint">Create a new instance of ReferencePlane.</param>
		/// <param name="endPoint">The free end apply to reference plane.</param>
		/// <param name="thirdPnt">A third point needed to define the reference plane.</param>
		public static void Distribute(Mesh mesh, ref Autodesk.Revit.DB.XYZ startPoint, ref Autodesk.Revit.DB.XYZ endPoint, ref Autodesk.Revit.DB.XYZ thirdPnt)
		{
			int count = System.Convert.ToInt32(mesh.Vertices.Count);
			startPoint = mesh.Vertices[0];
			endPoint = mesh.Vertices[count / 3];
			thirdPnt = mesh.Vertices[count / 3 * 2];
		}
		
		/// <summary>
		/// Calculate the length between two points.
		/// </summary>
		/// <param name="startPoint">The start point.</param>
		/// <param name="endPoint">The end point.</param>
		/// <returns>The length between two points.</returns>
		public static double GetLength(Autodesk.Revit.DB.XYZ startPoint, Autodesk.Revit.DB.XYZ endPoint)
		{
			return Math.Sqrt(Math.Pow(System.Convert.ToDouble(endPoint.X - startPoint.X), 2) + Math.Pow(System.Convert.ToDouble(endPoint.Y - startPoint.Y), 2) + Math.Pow(System.Convert.ToDouble(endPoint.Z - startPoint.Z), 2));
		}
		
		/// <summary>
		/// The distance between two value in a same axis.
		/// </summary>
		/// <param name="start">start value.</param>
		/// <param name="end">end value.</param>
		/// <returns>The distance between two value.</returns>
		public static double GetDistance(double start, double end)
		{
			return Math.Abs(start - end);
		}
		
		/// <summary>
		/// Get the vector between two points.
		/// </summary>
		/// <param name="startPoint">The start point.</param>
		/// <param name="endPoint">The end point.</param>
		/// <returns>The vector between two points.</returns>
		public static Autodesk.Revit.DB.XYZ GetVector(Autodesk.Revit.DB.XYZ startPoint, Autodesk.Revit.DB.XYZ endPoint)
		{
			return new Autodesk.Revit.DB.XYZ(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y, endPoint.Z - startPoint.Z);
		}
		
		/// <summary>
		/// Determines whether a face is vertical.
		/// </summary>
		/// <param name="face">The face to be determined.</param>
		/// <returns>Return true if this face is vertical, or else return false.</returns>
		private static bool IsVerticalFace(Face face)
		{
			foreach (EdgeArray ea in face.EdgeLoops)
			{
				foreach (Edge e in ea)
				{
					if (IsVerticalEdge(e))
					{
						return true;
					}
				}
			}
			
			return false;
		}
		
		/// <summary>
		/// Determines whether a edge is vertical.
		/// </summary>
		/// <param name="edge">The edge to be determined.</param>
		/// <returns>Return true if this edge is vertical, or else return false.</returns>
		private static bool IsVerticalEdge(Edge edge)
		{
			List<XYZ> polyline = edge.Tessellate() as List<XYZ>;
			Autodesk.Revit.DB.XYZ verticalVct = new Autodesk.Revit.DB.XYZ(0, 0, 1);
			Autodesk.Revit.DB.XYZ pointBuffer = polyline[0];
			
			for (int i = 1; i <= polyline.Count - 1; i++)
			{
				Autodesk.Revit.DB.XYZ temp = polyline[i];
				Autodesk.Revit.DB.XYZ vector = GetVector(pointBuffer, temp);
				if (Equal(vector, verticalVct))
				{
					return true;
				}
				else
				{
					continue;
				}
			}
			
			return false;
		}
		
		/// <summary>
		/// Determines whether two vector are equal in x and y axis.
		/// </summary>
		/// <param name="vectorA">The vector A.</param>
		/// <param name="vectorB">The vector B.</param>
		/// <returns>Return true if two vector are equals, or else return false.</returns>
		private static bool Equal(Autodesk.Revit.DB.XYZ vectorA, Autodesk.Revit.DB.XYZ vectorB)
		{
			bool isNotEqual = (VertexTolerance < Math.Abs(vectorA.X - vectorB.X)) || (VertexTolerance < Math.Abs(vectorA.Y - vectorB.Y));
			return (isNotEqual ? false : true);
		}
		
		/// <summary> 比较两个点之间的距离是否小于指定的容差 </summary>
		/// <remarks>对于Revit中的XYZ对象，其也有一个IsAlmostEqualTo函数，但是要注意，
		/// 那个函数是用来比较两个向量的方向是否小于指定的弧度容差。</remarks>
		public static bool IsAlmostEqualTo(XYZ Point1, XYZ Point2, double Precision)
		{
			double D = System.Math.Sqrt(System.Convert.ToDouble(Math.Pow((Point1.X - Point2.X), 2) + Math.Pow(
				(Point1.Y - Point2.Y), 2) + Math.Pow(
				(Point1.Z - Point2.Z), 2)));
			if (D <= Precision)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		/// <summary>
		/// 找到Extrusion中指定法向的平面。如果有多个平面的法向都是指定的法向，则返回第一个找到的平面。
		/// Given a solid, find a planar face with the given normal (version 2)
		/// this is a slightly enhanced version which checks if the face is on the given reference plane.
		/// </summary>
		/// <param name="refPlane">除了验证平面的法向外，还可以额外验证一下指定法向的平面是否是在指定的参考平面上。即要同时满足normal与ReferencePlane两个条件。
		/// additionally, we want to check if the face is on the reference plane</param>
		/// <remarks></remarks>
		public static PlanarFace FindFace(Extrusion aSolid, XYZ normal, ReferencePlane refPlane = null)
		{
			
			//' get the geometry object of the given element
			//'
			Options op = new Options();
			op.ComputeReferences = true;
			// Dim geomObjs As GeometryObjectArray = aSolid.Geometry(op).Objects
			
			//' loop through the array and find a face with the given normal
			//'
			foreach (GeometryObject geomObj in aSolid.get_Geometry(op))
			{
				
				if (geomObj is Solid) //'  solid is what we are interested in.
				{
					
					Solid pSolid = (Solid)geomObj;
					FaceArray faces = pSolid.Faces;
					
					foreach (Face pFace in faces)
					{
						if (pFace is PlanarFace)
						{
							PlanarFace pPlanarFace =(PlanarFace) pFace;
							if (!(pPlanarFace == null))
							{
								//'  check to see if they have same normal
								if (pPlanarFace.ComputeNormal(new UV(0, 0)).IsAlmostEqualTo(normal))
								{
									
									if (refPlane == null)
									{
										return pPlanarFace; //' we found the face.
									}
									else
									{
										//'  additionally, we want to check if the face is on the reference plane
										//'  get a point on the face. Any point will do.
										Edge pEdge = pPlanarFace.EdgeLoops.get_Item(0).get_Item(0);
										XYZ pt = pEdge.Evaluate(0.0);
										//'  is the point on the reference plane
										bool res = System.Convert.ToBoolean(IsPointOnPlane(pt, refPlane.GetPlane()));
										if (res)
										{
											return pPlanarFace; //' we found the face
										}
									}
								}
							}
						}
					}
					
				}
				else if (geomObj is GeometryInstance)
				{
					//' will come back later as needed.
					
				}
				else if (geomObj is Curve)
				{
					//' will come nack later as needed.
					
				}
				else if (geomObj is Mesh)
				{
					//' will come back later as needed.
					
				}
				else
				{
					//' what else do we have
					
				}
			}
			
			//' if we come here, we did not find any.
			return null;
			
		}
		
		/// <summary>
		/// 判断一个三维点是否在指定的平面上
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="plane"></param>
		/// <returns></returns>
		public static dynamic IsPointOnPlane(XYZ p1, Plane plane)
		{
			
			double dt = System.Convert.ToDouble((plane.Normal).DotProduct(p1 - (plane.Origin))); // 指定点到原点法向向量的投影长度，即此点到对应平面距离
			if (dt < VertexTolerance) // 说明此点到指定平面的距离很小
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		/// <summary>
		/// 判断一个三维点是否在指定的参考平面上
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="plane">参考平面，ReferencePlane.GetPlane方法也可以返回Plane对象。</param>
		/// <returns></returns>
		public static dynamic IsPointOnPlane(XYZ p1, ReferencePlane plane)
		{
			
			double dt = System.Convert.ToDouble((plane.Normal).DotProduct(p1 - (plane.BubbleEnd))); // 指定点到原点法向向量的投影长度，即此点到对应平面距离
			if (dt < VertexTolerance) // 说明此点到指定平面的距离很小
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
	}
	
}

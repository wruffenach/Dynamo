﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSRevitNodes.Elements
{
    class DSSketchPlane
    {
    }
}

/*

    [NodeName("Sketch Plane from Plane")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_CREATE)]
    [NodeDescription("Creates a geometric sketch plane.")]
    public class SketchPlane : RevitTransactionNodeWithOneOutput
    {
        public SketchPlane()
        {
            InPortData.Add(new PortData("plane", "The plane in which to define the sketch.", typeof(FScheme.Value.Container))); // SketchPlane can accept Plane, Reference or PlanarFace
            OutPortData.Add(new PortData("sketch plane", "SketchPlane", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        bool resetPlaneofSketchPlaneElement(Autodesk.Revit.DB.SketchPlane sp, Autodesk.Revit.DB.Plane p)
        {
            XYZ newOrigin = p.Origin;
            XYZ newNorm = p.Normal;
            var oldP = sp.Plane;
            XYZ oldOrigin = oldP.Origin;
            XYZ oldNorm = oldP.Normal;

            if (oldNorm.IsAlmostEqualTo(newNorm))
            {
                XYZ moveVec = newOrigin - oldOrigin;
                if (moveVec.GetLength() > 0.000000001)
                    ElementTransformUtils.MoveElement(this.UIDocument.Document, sp.Id, moveVec);
                return true;
            }
            //rotation might not work for sketch planes
            return false;
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var input = args[0];

            //TODO: If possible, update to handle mutation rather than deletion...
            //foreach (var e in this.Elements)
            //    this.DeleteElement(e);

            if (input.IsList)
            {
                //TODO: If possible, update to handle mutation rather than deletion...
                //but: how to preserve elements when list size changes or user reshuffles elements in the list?
                foreach (var e in this.Elements)
                    this.DeleteElement(e);

                var planeList = (input as FScheme.Value.List).Item;

                var result = Utils.SequenceToFSharpList(
                   planeList.Select(
                      delegate(FScheme.Value x)
                      {
                          Autodesk.Revit.DB.SketchPlane sp = null;

                          //handle Plane, Reference or PlanarFace, also test for family or project doc. there probably is a cleaner way to test for all these conditions.
                          if (((FScheme.Value.Container)x).Item is Autodesk.Revit.DB.Plane) //TODO: ensure this is correctly casting and testing.
                          {
                              sp = (this.UIDocument.Document.IsFamilyDocument)
                              ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(
                                 (Autodesk.Revit.DB.Plane)((FScheme.Value.Container)x).Item
                              )
                              : this.UIDocument.Document.Create.NewSketchPlane(
                                 (Autodesk.Revit.DB.Plane)((FScheme.Value.Container)x).Item
                              );
                          }
                          else if (((FScheme.Value.Container)x).Item is Reference)
                          {
                              sp = (this.UIDocument.Document.IsFamilyDocument)
                              ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(
                                 (Reference)((FScheme.Value.Container)x).Item
                              )
                              : this.UIDocument.Document.Create.NewSketchPlane(
                                 (Reference)((FScheme.Value.Container)x).Item
                              );
                          }
                          else if (((FScheme.Value.Container)x).Item is PlanarFace)
                          {
                              sp = (this.UIDocument.Document.IsFamilyDocument)
                              ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(
                                 (PlanarFace)((FScheme.Value.Container)x).Item
                              )
                              : this.UIDocument.Document.Create.NewSketchPlane(
                                 (PlanarFace)((FScheme.Value.Container)x).Item
                              );
                          }


                          this.Elements.Add(sp.Id);
                          return FScheme.Value.NewContainer(sp);
                      }
                   )
                );

                return FScheme.Value.NewList(result);
            }
            else
            {
                Autodesk.Revit.DB.SketchPlane sp = null;
                bool keepExistingElement = false;
                var x = ((FScheme.Value.Container)input).Item;

                //TODO: If possible, update to handle mutation rather than deletion...
                if (this.Elements.Count == 1)
                {
                    Element e = this.UIDocument.Document.GetElement(this.Elements[0]);
                    if (e != null && (e is Autodesk.Revit.DB.SketchPlane))
                    {
                        sp = (Autodesk.Revit.DB.SketchPlane)e;

                        if (x is Reference)
                            keepExistingElement = true;
                        else if (x is Autodesk.Revit.DB.Plane && resetPlaneofSketchPlaneElement(sp, (Autodesk.Revit.DB.Plane)x))
                            keepExistingElement = true;
                    }
                }
                if (!keepExistingElement)
                {
                    foreach (var e in this.Elements)
                        this.DeleteElement(e);

                    //handle Plane, Reference or PlanarFace, also test for family or project doc. there probably is a cleaner way to test for all these conditions.
                    if (x is Autodesk.Revit.DB.Plane)
                    {
                        Autodesk.Revit.DB.Plane p = x as Autodesk.Revit.DB.Plane;
                        sp = (this.UIDocument.Document.IsFamilyDocument)
                           ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(p)
                           : this.UIDocument.Document.Create.NewSketchPlane(p);
                    }
                    else if (x is Reference)
                    {
                        Reference r = x as Reference;
                        sp = (this.UIDocument.Document.IsFamilyDocument)
                           ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(r)
                           : this.UIDocument.Document.Create.NewSketchPlane(r);
                    }
                    else if (x is PlanarFace)
                    {
                        PlanarFace p = x as PlanarFace;
                        sp = (this.UIDocument.Document.IsFamilyDocument)
                           ? this.UIDocument.Document.FamilyCreate.NewSketchPlane(p)
                           : this.UIDocument.Document.Create.NewSketchPlane(p);
                    }

                    this.Elements.Add(sp.Id);
                }

                return FScheme.Value.NewContainer(sp);
            }
        }
    }

*/
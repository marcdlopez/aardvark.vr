namespace SimpleDrawingModel

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open SimpleDrawingModel

[<AutoOpen>]
module Mutable =

    
    
    type MSimpleDrawingModel(__initial : SimpleDrawingModel.SimpleDrawingModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<SimpleDrawingModel.SimpleDrawingModel> = Aardvark.Base.Incremental.EqModRef<SimpleDrawingModel.SimpleDrawingModel>(__initial) :> Aardvark.Base.Incremental.IModRef<SimpleDrawingModel.SimpleDrawingModel>
        let _camera = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.camera)
        let _drawingEnabled = ResetMod.Create(__initial.drawingEnabled)
        let _hoverPosition = MOption.Create(__initial.hoverPosition)
        let _drawing = Rabbyte.Drawing.Mutable.MDrawingModel.Create(__initial.drawing)
        let _annotations = Rabbyte.Annotation.Mutable.MAnnotationModel.Create(__initial.annotations)
        
        member x.camera = _camera
        member x.drawingEnabled = _drawingEnabled :> IMod<_>
        member x.hoverPosition = _hoverPosition :> IMod<_>
        member x.drawing = _drawing
        member x.annotations = _annotations
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : SimpleDrawingModel.SimpleDrawingModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                Aardvark.UI.Primitives.Mutable.MCameraControllerState.Update(_camera, v.camera)
                ResetMod.Update(_drawingEnabled,v.drawingEnabled)
                MOption.Update(_hoverPosition, v.hoverPosition)
                Rabbyte.Drawing.Mutable.MDrawingModel.Update(_drawing, v.drawing)
                Rabbyte.Annotation.Mutable.MAnnotationModel.Update(_annotations, v.annotations)
                
        
        static member Create(__initial : SimpleDrawingModel.SimpleDrawingModel) : MSimpleDrawingModel = MSimpleDrawingModel(__initial)
        static member Update(m : MSimpleDrawingModel, v : SimpleDrawingModel.SimpleDrawingModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<SimpleDrawingModel.SimpleDrawingModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module SimpleDrawingModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let camera =
                { new Lens<SimpleDrawingModel.SimpleDrawingModel, Aardvark.UI.Primitives.CameraControllerState>() with
                    override x.Get(r) = r.camera
                    override x.Set(r,v) = { r with camera = v }
                    override x.Update(r,f) = { r with camera = f r.camera }
                }
            let drawingEnabled =
                { new Lens<SimpleDrawingModel.SimpleDrawingModel, System.Boolean>() with
                    override x.Get(r) = r.drawingEnabled
                    override x.Set(r,v) = { r with drawingEnabled = v }
                    override x.Update(r,f) = { r with drawingEnabled = f r.drawingEnabled }
                }
            let hoverPosition =
                { new Lens<SimpleDrawingModel.SimpleDrawingModel, Microsoft.FSharp.Core.Option<Aardvark.Base.Trafo3d>>() with
                    override x.Get(r) = r.hoverPosition
                    override x.Set(r,v) = { r with hoverPosition = v }
                    override x.Update(r,f) = { r with hoverPosition = f r.hoverPosition }
                }
            let drawing =
                { new Lens<SimpleDrawingModel.SimpleDrawingModel, Rabbyte.Drawing.DrawingModel>() with
                    override x.Get(r) = r.drawing
                    override x.Set(r,v) = { r with drawing = v }
                    override x.Update(r,f) = { r with drawing = f r.drawing }
                }
            let annotations =
                { new Lens<SimpleDrawingModel.SimpleDrawingModel, Rabbyte.Annotation.AnnotationModel>() with
                    override x.Get(r) = r.annotations
                    override x.Set(r,v) = { r with annotations = v }
                    override x.Update(r,f) = { r with annotations = f r.annotations }
                }

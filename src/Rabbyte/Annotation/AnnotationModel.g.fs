namespace Rabbyte.Annotation

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Rabbyte.Annotation

[<AutoOpen>]
module Mutable =

    
    
    type MAnnotation(__initial : Rabbyte.Annotation.Annotation) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Rabbyte.Annotation.Annotation> = Aardvark.Base.Incremental.EqModRef<Rabbyte.Annotation.Annotation>(__initial) :> Aardvark.Base.Incremental.IModRef<Rabbyte.Annotation.Annotation>
        let _version = ResetMod.Create(__initial.version)
        let _modelTrafo = ResetMod.Create(__initial.modelTrafo)
        let _points = MList.Create(__initial.points)
        let _segments = MList.Create(__initial.segments)
        let _style = Rabbyte.Drawing.Mutable.MBrushStyle.Create(__initial.style)
        let _primitiveType = ResetMod.Create(__initial.primitiveType)
        let _clippingVolume = ResetMod.Create(__initial.clippingVolume)
        let _visible = ResetMod.Create(__initial.visible)
        let _text = ResetMod.Create(__initial.text)
        let _textsize = ResetMod.Create(__initial.textsize)
        let _surfaceName = ResetMod.Create(__initial.surfaceName)
        
        member x.version = _version :> IMod<_>
        member x.key = __current.Value.key
        member x.modelTrafo = _modelTrafo :> IMod<_>
        member x.points = _points :> alist<_>
        member x.segments = _segments :> alist<_>
        member x.style = _style
        member x.primitiveType = _primitiveType :> IMod<_>
        member x.clippingVolume = _clippingVolume :> IMod<_>
        member x.visible = _visible :> IMod<_>
        member x.text = _text :> IMod<_>
        member x.textsize = _textsize :> IMod<_>
        member x.surfaceName = _surfaceName :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Rabbyte.Annotation.Annotation) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_version,v.version)
                ResetMod.Update(_modelTrafo,v.modelTrafo)
                MList.Update(_points, v.points)
                MList.Update(_segments, v.segments)
                Rabbyte.Drawing.Mutable.MBrushStyle.Update(_style, v.style)
                ResetMod.Update(_primitiveType,v.primitiveType)
                ResetMod.Update(_clippingVolume,v.clippingVolume)
                ResetMod.Update(_visible,v.visible)
                ResetMod.Update(_text,v.text)
                ResetMod.Update(_textsize,v.textsize)
                ResetMod.Update(_surfaceName,v.surfaceName)
                
        
        static member Create(__initial : Rabbyte.Annotation.Annotation) : MAnnotation = MAnnotation(__initial)
        static member Update(m : MAnnotation, v : Rabbyte.Annotation.Annotation) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Rabbyte.Annotation.Annotation> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Annotation =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let version =
                { new Lens<Rabbyte.Annotation.Annotation, System.Int32>() with
                    override x.Get(r) = r.version
                    override x.Set(r,v) = { r with version = v }
                    override x.Update(r,f) = { r with version = f r.version }
                }
            let key =
                { new Lens<Rabbyte.Annotation.Annotation, System.Guid>() with
                    override x.Get(r) = r.key
                    override x.Set(r,v) = { r with key = v }
                    override x.Update(r,f) = { r with key = f r.key }
                }
            let modelTrafo =
                { new Lens<Rabbyte.Annotation.Annotation, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.modelTrafo
                    override x.Set(r,v) = { r with modelTrafo = v }
                    override x.Update(r,f) = { r with modelTrafo = f r.modelTrafo }
                }
            let points =
                { new Lens<Rabbyte.Annotation.Annotation, Aardvark.Base.plist<Aardvark.Base.V3d>>() with
                    override x.Get(r) = r.points
                    override x.Set(r,v) = { r with points = v }
                    override x.Update(r,f) = { r with points = f r.points }
                }
            let segments =
                { new Lens<Rabbyte.Annotation.Annotation, Aardvark.Base.plist<Rabbyte.Drawing.Segment>>() with
                    override x.Get(r) = r.segments
                    override x.Set(r,v) = { r with segments = v }
                    override x.Update(r,f) = { r with segments = f r.segments }
                }
            let style =
                { new Lens<Rabbyte.Annotation.Annotation, Rabbyte.Drawing.BrushStyle>() with
                    override x.Get(r) = r.style
                    override x.Set(r,v) = { r with style = v }
                    override x.Update(r,f) = { r with style = f r.style }
                }
            let primitiveType =
                { new Lens<Rabbyte.Annotation.Annotation, Rabbyte.Drawing.PrimitiveType>() with
                    override x.Get(r) = r.primitiveType
                    override x.Set(r,v) = { r with primitiveType = v }
                    override x.Update(r,f) = { r with primitiveType = f r.primitiveType }
                }
            let clippingVolume =
                { new Lens<Rabbyte.Annotation.Annotation, Rabbyte.Annotation.ClippingVolumeType>() with
                    override x.Get(r) = r.clippingVolume
                    override x.Set(r,v) = { r with clippingVolume = v }
                    override x.Update(r,f) = { r with clippingVolume = f r.clippingVolume }
                }
            let visible =
                { new Lens<Rabbyte.Annotation.Annotation, System.Boolean>() with
                    override x.Get(r) = r.visible
                    override x.Set(r,v) = { r with visible = v }
                    override x.Update(r,f) = { r with visible = f r.visible }
                }
            let text =
                { new Lens<Rabbyte.Annotation.Annotation, System.String>() with
                    override x.Get(r) = r.text
                    override x.Set(r,v) = { r with text = v }
                    override x.Update(r,f) = { r with text = f r.text }
                }
            let textsize =
                { new Lens<Rabbyte.Annotation.Annotation, System.Double>() with
                    override x.Get(r) = r.textsize
                    override x.Set(r,v) = { r with textsize = v }
                    override x.Update(r,f) = { r with textsize = f r.textsize }
                }
            let surfaceName =
                { new Lens<Rabbyte.Annotation.Annotation, System.String>() with
                    override x.Get(r) = r.surfaceName
                    override x.Set(r,v) = { r with surfaceName = v }
                    override x.Update(r,f) = { r with surfaceName = f r.surfaceName }
                }
    
    
    type MAnnotationModel(__initial : Rabbyte.Annotation.AnnotationModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Rabbyte.Annotation.AnnotationModel> = Aardvark.Base.Incremental.EqModRef<Rabbyte.Annotation.AnnotationModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Rabbyte.Annotation.AnnotationModel>
        let _annotations = MList.Create(__initial.annotations, (fun v -> MAnnotation.Create(v)), (fun (m,v) -> MAnnotation.Update(m, v)), (fun v -> v))
        let _annotationsGrouped = MMap.Create(__initial.annotationsGrouped, (fun v -> MList.Create(v, (fun v -> MAnnotation.Create(v)), (fun (m,v) -> MAnnotation.Update(m, v)), (fun v -> v))), (fun (m,v) -> MList.Update(m, v)), (fun v -> v :> alist<_>))
        let _showDebug = ResetMod.Create(__initial.showDebug)
        let _extrusionOffset = ResetMod.Create(__initial.extrusionOffset)
        
        member x.annotations = _annotations :> alist<_>
        member x.annotationsGrouped = _annotationsGrouped :> amap<_,_>
        member x.showDebug = _showDebug :> IMod<_>
        member x.extrusionOffset = _extrusionOffset :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Rabbyte.Annotation.AnnotationModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MList.Update(_annotations, v.annotations)
                MMap.Update(_annotationsGrouped, v.annotationsGrouped)
                ResetMod.Update(_showDebug,v.showDebug)
                ResetMod.Update(_extrusionOffset,v.extrusionOffset)
                
        
        static member Create(__initial : Rabbyte.Annotation.AnnotationModel) : MAnnotationModel = MAnnotationModel(__initial)
        static member Update(m : MAnnotationModel, v : Rabbyte.Annotation.AnnotationModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Rabbyte.Annotation.AnnotationModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module AnnotationModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let annotations =
                { new Lens<Rabbyte.Annotation.AnnotationModel, Aardvark.Base.plist<Rabbyte.Annotation.Annotation>>() with
                    override x.Get(r) = r.annotations
                    override x.Set(r,v) = { r with annotations = v }
                    override x.Update(r,f) = { r with annotations = f r.annotations }
                }
            let annotationsGrouped =
                { new Lens<Rabbyte.Annotation.AnnotationModel, Aardvark.Base.hmap<Aardvark.Base.C4b,Aardvark.Base.plist<Rabbyte.Annotation.Annotation>>>() with
                    override x.Get(r) = r.annotationsGrouped
                    override x.Set(r,v) = { r with annotationsGrouped = v }
                    override x.Update(r,f) = { r with annotationsGrouped = f r.annotationsGrouped }
                }
            let showDebug =
                { new Lens<Rabbyte.Annotation.AnnotationModel, System.Boolean>() with
                    override x.Get(r) = r.showDebug
                    override x.Set(r,v) = { r with showDebug = v }
                    override x.Update(r,f) = { r with showDebug = f r.showDebug }
                }
            let extrusionOffset =
                { new Lens<Rabbyte.Annotation.AnnotationModel, System.Double>() with
                    override x.Get(r) = r.extrusionOffset
                    override x.Set(r,v) = { r with extrusionOffset = v }
                    override x.Update(r,f) = { r with extrusionOffset = f r.extrusionOffset }
                }

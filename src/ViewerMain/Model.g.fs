namespace OpcSelectionViewer

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open OpcSelectionViewer

[<AutoOpen>]
module Mutable =

    
    
    type MAxis(__initial : OpcSelectionViewer.Axis) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<OpcSelectionViewer.Axis> = Aardvark.Base.Incremental.EqModRef<OpcSelectionViewer.Axis>(__initial) :> Aardvark.Base.Incremental.IModRef<OpcSelectionViewer.Axis>
        let _positions = ResetMod.Create(__initial.positions)
        let _selectionOnAxis = MOption.Create(__initial.selectionOnAxis)
        let _pointList = MList.Create(__initial.pointList)
        let _length = ResetMod.Create(__initial.length)
        let _rangeSv = ResetMod.Create(__initial.rangeSv)
        
        member x.positions = _positions :> IMod<_>
        member x.selectionOnAxis = _selectionOnAxis :> IMod<_>
        member x.pointList = _pointList :> alist<_>
        member x.length = _length :> IMod<_>
        member x.rangeSv = _rangeSv :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : OpcSelectionViewer.Axis) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_positions,v.positions)
                MOption.Update(_selectionOnAxis, v.selectionOnAxis)
                MList.Update(_pointList, v.pointList)
                ResetMod.Update(_length,v.length)
                ResetMod.Update(_rangeSv,v.rangeSv)
                
        
        static member Create(__initial : OpcSelectionViewer.Axis) : MAxis = MAxis(__initial)
        static member Update(m : MAxis, v : OpcSelectionViewer.Axis) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<OpcSelectionViewer.Axis> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Axis =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let positions =
                { new Lens<OpcSelectionViewer.Axis, Microsoft.FSharp.Collections.List<Aardvark.Base.V3d>>() with
                    override x.Get(r) = r.positions
                    override x.Set(r,v) = { r with positions = v }
                    override x.Update(r,f) = { r with positions = f r.positions }
                }
            let selectionOnAxis =
                { new Lens<OpcSelectionViewer.Axis, Microsoft.FSharp.Core.Option<Aardvark.Base.V3d>>() with
                    override x.Get(r) = r.selectionOnAxis
                    override x.Set(r,v) = { r with selectionOnAxis = v }
                    override x.Update(r,f) = { r with selectionOnAxis = f r.selectionOnAxis }
                }
            let pointList =
                { new Lens<OpcSelectionViewer.Axis, Aardvark.Base.plist<OpcSelectionViewer.OrientedPoint>>() with
                    override x.Get(r) = r.pointList
                    override x.Set(r,v) = { r with pointList = v }
                    override x.Update(r,f) = { r with pointList = f r.pointList }
                }
            let length =
                { new Lens<OpcSelectionViewer.Axis, System.Double>() with
                    override x.Get(r) = r.length
                    override x.Set(r,v) = { r with length = v }
                    override x.Update(r,f) = { r with length = f r.length }
                }
            let rangeSv =
                { new Lens<OpcSelectionViewer.Axis, Aardvark.Base.Range1d>() with
                    override x.Get(r) = r.rangeSv
                    override x.Set(r,v) = { r with rangeSv = v }
                    override x.Update(r,f) = { r with rangeSv = f r.rangeSv }
                }
    
    
    type MModel(__initial : OpcSelectionViewer.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<OpcSelectionViewer.Model> = Aardvark.Base.Incremental.EqModRef<OpcSelectionViewer.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<OpcSelectionViewer.Model>
        let _cameraState = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.cameraState)
        let _mainFrustum = ResetMod.Create(__initial.mainFrustum)
        let _fillMode = ResetMod.Create(__initial.fillMode)
        let _boundingBox = ResetMod.Create(__initial.boundingBox)
        let _axis = MOption.Create(__initial.axis, (fun v -> MAxis.Create(v)), (fun (m,v) -> MAxis.Update(m, v)), (fun v -> v))
        let _boxes = ResetMod.Create(__initial.boxes)
        let _opcInfos = MMap.Create(__initial.opcInfos, (fun v -> OpcViewer.Base.Picking.Mutable.MOpcData.Create(v)), (fun (m,v) -> OpcViewer.Base.Picking.Mutable.MOpcData.Update(m, v)), (fun v -> v))
        let _threads = ResetMod.Create(__initial.threads)
        let _dockConfig = ResetMod.Create(__initial.dockConfig)
        let _picking = OpcViewer.Base.Picking.Mutable.MPickingModel.Create(__initial.picking)
        let _pickingActive = ResetMod.Create(__initial.pickingActive)
        let _opcAttributes = OpcViewer.Base.Attributes.Mutable.MAttributeModel.Create(__initial.opcAttributes)
        let _drawing = Rabbyte.Drawing.Mutable.MDrawingModel.Create(__initial.drawing)
        let _annotations = Rabbyte.Annotation.Mutable.MAnnotationModel.Create(__initial.annotations)
        
        member x.cameraState = _cameraState
        member x.mainFrustum = _mainFrustum :> IMod<_>
        member x.fillMode = _fillMode :> IMod<_>
        member x.patchHierarchies = __current.Value.patchHierarchies
        member x.boundingBox = _boundingBox :> IMod<_>
        member x.axis = _axis :> IMod<_>
        member x.boxes = _boxes :> IMod<_>
        member x.opcInfos = _opcInfos :> amap<_,_>
        member x.threads = _threads :> IMod<_>
        member x.dockConfig = _dockConfig :> IMod<_>
        member x.picking = _picking
        member x.pickingActive = _pickingActive :> IMod<_>
        member x.opcAttributes = _opcAttributes
        member x.drawing = _drawing
        member x.annotations = _annotations
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : OpcSelectionViewer.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                Aardvark.UI.Primitives.Mutable.MCameraControllerState.Update(_cameraState, v.cameraState)
                ResetMod.Update(_mainFrustum,v.mainFrustum)
                ResetMod.Update(_fillMode,v.fillMode)
                ResetMod.Update(_boundingBox,v.boundingBox)
                MOption.Update(_axis, v.axis)
                ResetMod.Update(_boxes,v.boxes)
                MMap.Update(_opcInfos, v.opcInfos)
                ResetMod.Update(_threads,v.threads)
                ResetMod.Update(_dockConfig,v.dockConfig)
                OpcViewer.Base.Picking.Mutable.MPickingModel.Update(_picking, v.picking)
                ResetMod.Update(_pickingActive,v.pickingActive)
                OpcViewer.Base.Attributes.Mutable.MAttributeModel.Update(_opcAttributes, v.opcAttributes)
                Rabbyte.Drawing.Mutable.MDrawingModel.Update(_drawing, v.drawing)
                Rabbyte.Annotation.Mutable.MAnnotationModel.Update(_annotations, v.annotations)
                
        
        static member Create(__initial : OpcSelectionViewer.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : OpcSelectionViewer.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<OpcSelectionViewer.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let cameraState =
                { new Lens<OpcSelectionViewer.Model, Aardvark.UI.Primitives.CameraControllerState>() with
                    override x.Get(r) = r.cameraState
                    override x.Set(r,v) = { r with cameraState = v }
                    override x.Update(r,f) = { r with cameraState = f r.cameraState }
                }
            let mainFrustum =
                { new Lens<OpcSelectionViewer.Model, Aardvark.Base.Frustum>() with
                    override x.Get(r) = r.mainFrustum
                    override x.Set(r,v) = { r with mainFrustum = v }
                    override x.Update(r,f) = { r with mainFrustum = f r.mainFrustum }
                }
            let fillMode =
                { new Lens<OpcSelectionViewer.Model, Aardvark.Base.Rendering.FillMode>() with
                    override x.Get(r) = r.fillMode
                    override x.Set(r,v) = { r with fillMode = v }
                    override x.Update(r,f) = { r with fillMode = f r.fillMode }
                }
            let patchHierarchies =
                { new Lens<OpcSelectionViewer.Model, Microsoft.FSharp.Collections.List<Aardvark.SceneGraph.Opc.PatchHierarchy>>() with
                    override x.Get(r) = r.patchHierarchies
                    override x.Set(r,v) = { r with patchHierarchies = v }
                    override x.Update(r,f) = { r with patchHierarchies = f r.patchHierarchies }
                }
            let boundingBox =
                { new Lens<OpcSelectionViewer.Model, Aardvark.Base.Box3d>() with
                    override x.Get(r) = r.boundingBox
                    override x.Set(r,v) = { r with boundingBox = v }
                    override x.Update(r,f) = { r with boundingBox = f r.boundingBox }
                }
            let axis =
                { new Lens<OpcSelectionViewer.Model, Microsoft.FSharp.Core.Option<OpcSelectionViewer.Axis>>() with
                    override x.Get(r) = r.axis
                    override x.Set(r,v) = { r with axis = v }
                    override x.Update(r,f) = { r with axis = f r.axis }
                }
            let boxes =
                { new Lens<OpcSelectionViewer.Model, Microsoft.FSharp.Collections.List<Aardvark.Base.Box3d>>() with
                    override x.Get(r) = r.boxes
                    override x.Set(r,v) = { r with boxes = v }
                    override x.Update(r,f) = { r with boxes = f r.boxes }
                }
            let opcInfos =
                { new Lens<OpcSelectionViewer.Model, Aardvark.Base.hmap<Aardvark.Base.Box3d,OpcViewer.Base.Picking.OpcData>>() with
                    override x.Get(r) = r.opcInfos
                    override x.Set(r,v) = { r with opcInfos = v }
                    override x.Update(r,f) = { r with opcInfos = f r.opcInfos }
                }
            let threads =
                { new Lens<OpcSelectionViewer.Model, Aardvark.Base.Incremental.ThreadPool<OpcSelectionViewer.Message>>() with
                    override x.Get(r) = r.threads
                    override x.Set(r,v) = { r with threads = v }
                    override x.Update(r,f) = { r with threads = f r.threads }
                }
            let dockConfig =
                { new Lens<OpcSelectionViewer.Model, Aardvark.UI.Primitives.DockConfig>() with
                    override x.Get(r) = r.dockConfig
                    override x.Set(r,v) = { r with dockConfig = v }
                    override x.Update(r,f) = { r with dockConfig = f r.dockConfig }
                }
            let picking =
                { new Lens<OpcSelectionViewer.Model, OpcViewer.Base.Picking.PickingModel>() with
                    override x.Get(r) = r.picking
                    override x.Set(r,v) = { r with picking = v }
                    override x.Update(r,f) = { r with picking = f r.picking }
                }
            let pickingActive =
                { new Lens<OpcSelectionViewer.Model, System.Boolean>() with
                    override x.Get(r) = r.pickingActive
                    override x.Set(r,v) = { r with pickingActive = v }
                    override x.Update(r,f) = { r with pickingActive = f r.pickingActive }
                }
            let opcAttributes =
                { new Lens<OpcSelectionViewer.Model, OpcViewer.Base.Attributes.AttributeModel>() with
                    override x.Get(r) = r.opcAttributes
                    override x.Set(r,v) = { r with opcAttributes = v }
                    override x.Update(r,f) = { r with opcAttributes = f r.opcAttributes }
                }
            let drawing =
                { new Lens<OpcSelectionViewer.Model, Rabbyte.Drawing.DrawingModel>() with
                    override x.Get(r) = r.drawing
                    override x.Set(r,v) = { r with drawing = v }
                    override x.Update(r,f) = { r with drawing = f r.drawing }
                }
            let annotations =
                { new Lens<OpcSelectionViewer.Model, Rabbyte.Annotation.AnnotationModel>() with
                    override x.Get(r) = r.annotations
                    override x.Set(r,v) = { r with annotations = v }
                    override x.Update(r,f) = { r with annotations = f r.annotations }
                }

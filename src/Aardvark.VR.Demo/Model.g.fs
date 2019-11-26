namespace Demo.Main

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Demo.Main

[<AutoOpen>]
module Mutable =

    
    
    type MPolygon(__initial : Demo.Main.Polygon) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Demo.Main.Polygon> = Aardvark.Base.Incremental.EqModRef<Demo.Main.Polygon>(__initial) :> Aardvark.Base.Incremental.IModRef<Demo.Main.Polygon>
        let _vertices = MList.Create(__initial.vertices)
        
        member x.vertices = _vertices :> alist<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Demo.Main.Polygon) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MList.Update(_vertices, v.vertices)
                
        
        static member Create(__initial : Demo.Main.Polygon) : MPolygon = MPolygon(__initial)
        static member Update(m : MPolygon, v : Demo.Main.Polygon) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Demo.Main.Polygon> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Polygon =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let vertices =
                { new Lens<Demo.Main.Polygon, Aardvark.Base.plist<Aardvark.Base.V3d>>() with
                    override x.Get(r) = r.vertices
                    override x.Set(r,v) = { r with vertices = v }
                    override x.Update(r,f) = { r with vertices = f r.vertices }
                }
    
    
    type MFinishedLine(__initial : Demo.Main.FinishedLine) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Demo.Main.FinishedLine> = Aardvark.Base.Incremental.EqModRef<Demo.Main.FinishedLine>(__initial) :> Aardvark.Base.Incremental.IModRef<Demo.Main.FinishedLine>
        let _points = MList.Create(__initial.points)
        let _trafo = ResetMod.Create(__initial.trafo)
        let _colorLine = ResetMod.Create(__initial.colorLine)
        let _colorVertices = ResetMod.Create(__initial.colorVertices)
        
        member x.points = _points :> alist<_>
        member x.trafo = _trafo :> IMod<_>
        member x.colorLine = _colorLine :> IMod<_>
        member x.colorVertices = _colorVertices :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Demo.Main.FinishedLine) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MList.Update(_points, v.points)
                ResetMod.Update(_trafo,v.trafo)
                ResetMod.Update(_colorLine,v.colorLine)
                ResetMod.Update(_colorVertices,v.colorVertices)
                
        
        static member Create(__initial : Demo.Main.FinishedLine) : MFinishedLine = MFinishedLine(__initial)
        static member Update(m : MFinishedLine, v : Demo.Main.FinishedLine) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Demo.Main.FinishedLine> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module FinishedLine =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let points =
                { new Lens<Demo.Main.FinishedLine, Aardvark.Base.plist<Demo.Main.LinePoints>>() with
                    override x.Get(r) = r.points
                    override x.Set(r,v) = { r with points = v }
                    override x.Update(r,f) = { r with points = f r.points }
                }
            let trafo =
                { new Lens<Demo.Main.FinishedLine, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.trafo
                    override x.Set(r,v) = { r with trafo = v }
                    override x.Update(r,f) = { r with trafo = f r.trafo }
                }
            let colorLine =
                { new Lens<Demo.Main.FinishedLine, Aardvark.Base.C4b>() with
                    override x.Get(r) = r.colorLine
                    override x.Set(r,v) = { r with colorLine = v }
                    override x.Update(r,f) = { r with colorLine = f r.colorLine }
                }
            let colorVertices =
                { new Lens<Demo.Main.FinishedLine, Aardvark.Base.C4b>() with
                    override x.Get(r) = r.colorVertices
                    override x.Set(r,v) = { r with colorVertices = v }
                    override x.Update(r,f) = { r with colorVertices = f r.colorVertices }
                }
    
    
    type MModel(__initial : Demo.Main.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Demo.Main.Model> = Aardvark.Base.Incremental.EqModRef<Demo.Main.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<Demo.Main.Model>
        let _text = ResetMod.Create(__initial.text)
        let _vr = ResetMod.Create(__initial.vr)
        let _cameraState = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.cameraState)
        let _ControllerPosition = ResetMod.Create(__initial.ControllerPosition)
        let _offsetToCenter = ResetMod.Create(__initial.offsetToCenter)
        let _controllerInfos = MMap.Create(__initial.controllerInfos, (fun v -> Demo.Mutable.MControllerInfo.Create(v)), (fun (m,v) -> Demo.Mutable.MControllerInfo.Update(m, v)), (fun v -> v))
        let _offsetControllerDistance = ResetMod.Create(__initial.offsetControllerDistance)
        let _boundingBox = ResetMod.Create(__initial.boundingBox)
        let _rotationAxis = ResetMod.Create(__initial.rotationAxis)
        let _opcInfos = MMap.Create(__initial.opcInfos, (fun v -> OpcViewer.Base.Picking.Mutable.MOpcData.Create(v)), (fun (m,v) -> OpcViewer.Base.Picking.Mutable.MOpcData.Update(m, v)), (fun v -> v))
        let _opcAttributes = OpcViewer.Base.Attributes.Mutable.MAttributeModel.Create(__initial.opcAttributes)
        let _mainFrustum = ResetMod.Create(__initial.mainFrustum)
        let _rotateBox = ResetMod.Create(__initial.rotateBox)
        let _pickingModel = OpcViewer.Base.Picking.Mutable.MPickingModel.Create(__initial.pickingModel)
        let _initWorkSpaceTrafo = ResetMod.Create(__initial.initWorkSpaceTrafo)
        let _workSpaceTrafo = ResetMod.Create(__initial.workSpaceTrafo)
        let _opcSpaceTrafo = ResetMod.Create(__initial.opcSpaceTrafo)
        let _annotationSpaceTrafo = ResetMod.Create(__initial.annotationSpaceTrafo)
        let _initOpcSpaceTrafo = ResetMod.Create(__initial.initOpcSpaceTrafo)
        let _initAnnotationSpaceTrafo = ResetMod.Create(__initial.initAnnotationSpaceTrafo)
        let _initControlTrafo = ResetMod.Create(__initial.initControlTrafo)
        let _init2ControlTrafo = ResetMod.Create(__initial.init2ControlTrafo)
        let _menuModel = Demo.Menu.Mutable.MMenuModel.Create(__initial.menuModel)
        let _drawingPoint = MList.Create(__initial.drawingPoint, (fun v -> Demo.Mutable.MVisibleBox.Create(v)), (fun (m,v) -> Demo.Mutable.MVisibleBox.Update(m, v)), (fun v -> v))
        let _drawingLine = ResetMod.Create(__initial.drawingLine)
        let _currentlyDrawing = MOption.Create(__initial.currentlyDrawing, (fun v -> MPolygon.Create(v)), (fun (m,v) -> MPolygon.Update(m, v)), (fun v -> v))
        let _finishedDrawings = MMap.Create(__initial.finishedDrawings, (fun v -> MPolygon.Create(v)), (fun (m,v) -> MPolygon.Update(m, v)), (fun v -> v))
        let _flagOnController = MList.Create(__initial.flagOnController, (fun v -> Demo.Mutable.MVisibleBox.Create(v)), (fun (m,v) -> Demo.Mutable.MVisibleBox.Update(m, v)), (fun v -> v))
        let _flagOnAnnotationSpace = MList.Create(__initial.flagOnAnnotationSpace, (fun v -> Demo.Mutable.MVisibleBox.Create(v)), (fun (m,v) -> Demo.Mutable.MVisibleBox.Update(m, v)), (fun v -> v))
        let _lineOnController = MList.Create(__initial.lineOnController, (fun v -> Demo.Mutable.MVisibleSphere.Create(v)), (fun (m,v) -> Demo.Mutable.MVisibleSphere.Update(m, v)), (fun v -> v))
        let _lineOnAnnotationSpace = MList.Create(__initial.lineOnAnnotationSpace, (fun v -> Demo.Mutable.MVisibleSphere.Create(v)), (fun (m,v) -> Demo.Mutable.MVisibleSphere.Update(m, v)), (fun v -> v))
        let _lineIsHovered = ResetMod.Create(__initial.lineIsHovered)
        let _lineMarsDisplay = ResetMod.Create(__initial.lineMarsDisplay)
        let _finishedLine = MMap.Create(__initial.finishedLine, (fun v -> MFinishedLine.Create(v)), (fun (m,v) -> MFinishedLine.Update(m, v)), (fun v -> v))
        let _dipAndStrikeOnController = MList.Create(__initial.dipAndStrikeOnController, (fun v -> Demo.Mutable.MVisibleCylinder.Create(v)), (fun (m,v) -> Demo.Mutable.MVisibleCylinder.Update(m, v)), (fun v -> v))
        let _dipAndStrikeOnAnnotationSpace = MList.Create(__initial.dipAndStrikeOnAnnotationSpace, (fun v -> Demo.Mutable.MVisibleCylinder.Create(v)), (fun (m,v) -> Demo.Mutable.MVisibleCylinder.Update(m, v)), (fun v -> v))
        let _dipAndStrikeAngle = ResetMod.Create(__initial.dipAndStrikeAngle)
        
        member x.text = _text :> IMod<_>
        member x.vr = _vr :> IMod<_>
        member x.cameraState = _cameraState
        member x.ControllerPosition = _ControllerPosition :> IMod<_>
        member x.offsetToCenter = _offsetToCenter :> IMod<_>
        member x.controllerInfos = _controllerInfos :> amap<_,_>
        member x.offsetControllerDistance = _offsetControllerDistance :> IMod<_>
        member x.patchHierarchies = __current.Value.patchHierarchies
        member x.boundingBox = _boundingBox :> IMod<_>
        member x.rotationAxis = _rotationAxis :> IMod<_>
        member x.opcInfos = _opcInfos :> amap<_,_>
        member x.opcAttributes = _opcAttributes
        member x.mainFrustum = _mainFrustum :> IMod<_>
        member x.rotateBox = _rotateBox :> IMod<_>
        member x.pickingModel = _pickingModel
        member x.initWorkSpaceTrafo = _initWorkSpaceTrafo :> IMod<_>
        member x.workSpaceTrafo = _workSpaceTrafo :> IMod<_>
        member x.opcSpaceTrafo = _opcSpaceTrafo :> IMod<_>
        member x.annotationSpaceTrafo = _annotationSpaceTrafo :> IMod<_>
        member x.initOpcSpaceTrafo = _initOpcSpaceTrafo :> IMod<_>
        member x.initAnnotationSpaceTrafo = _initAnnotationSpaceTrafo :> IMod<_>
        member x.initControlTrafo = _initControlTrafo :> IMod<_>
        member x.init2ControlTrafo = _init2ControlTrafo :> IMod<_>
        member x.menuModel = _menuModel
        member x.drawingPoint = _drawingPoint :> alist<_>
        member x.drawingLine = _drawingLine :> IMod<_>
        member x.currentlyDrawing = _currentlyDrawing :> IMod<_>
        member x.finishedDrawings = _finishedDrawings :> amap<_,_>
        member x.flagOnController = _flagOnController :> alist<_>
        member x.flagOnAnnotationSpace = _flagOnAnnotationSpace :> alist<_>
        member x.lineOnController = _lineOnController :> alist<_>
        member x.lineOnAnnotationSpace = _lineOnAnnotationSpace :> alist<_>
        member x.lineIsHovered = _lineIsHovered :> IMod<_>
        member x.lineMarsDisplay = _lineMarsDisplay :> IMod<_>
        member x.finishedLine = _finishedLine :> amap<_,_>
        member x.dipAndStrikeOnController = _dipAndStrikeOnController :> alist<_>
        member x.dipAndStrikeOnAnnotationSpace = _dipAndStrikeOnAnnotationSpace :> alist<_>
        member x.dipAndStrikeAngle = _dipAndStrikeAngle :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Demo.Main.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_text,v.text)
                ResetMod.Update(_vr,v.vr)
                Aardvark.UI.Primitives.Mutable.MCameraControllerState.Update(_cameraState, v.cameraState)
                ResetMod.Update(_ControllerPosition,v.ControllerPosition)
                ResetMod.Update(_offsetToCenter,v.offsetToCenter)
                MMap.Update(_controllerInfos, v.controllerInfos)
                ResetMod.Update(_offsetControllerDistance,v.offsetControllerDistance)
                ResetMod.Update(_boundingBox,v.boundingBox)
                ResetMod.Update(_rotationAxis,v.rotationAxis)
                MMap.Update(_opcInfos, v.opcInfos)
                OpcViewer.Base.Attributes.Mutable.MAttributeModel.Update(_opcAttributes, v.opcAttributes)
                ResetMod.Update(_mainFrustum,v.mainFrustum)
                ResetMod.Update(_rotateBox,v.rotateBox)
                OpcViewer.Base.Picking.Mutable.MPickingModel.Update(_pickingModel, v.pickingModel)
                ResetMod.Update(_initWorkSpaceTrafo,v.initWorkSpaceTrafo)
                ResetMod.Update(_workSpaceTrafo,v.workSpaceTrafo)
                ResetMod.Update(_opcSpaceTrafo,v.opcSpaceTrafo)
                ResetMod.Update(_annotationSpaceTrafo,v.annotationSpaceTrafo)
                ResetMod.Update(_initOpcSpaceTrafo,v.initOpcSpaceTrafo)
                ResetMod.Update(_initAnnotationSpaceTrafo,v.initAnnotationSpaceTrafo)
                ResetMod.Update(_initControlTrafo,v.initControlTrafo)
                ResetMod.Update(_init2ControlTrafo,v.init2ControlTrafo)
                Demo.Menu.Mutable.MMenuModel.Update(_menuModel, v.menuModel)
                MList.Update(_drawingPoint, v.drawingPoint)
                ResetMod.Update(_drawingLine,v.drawingLine)
                MOption.Update(_currentlyDrawing, v.currentlyDrawing)
                MMap.Update(_finishedDrawings, v.finishedDrawings)
                MList.Update(_flagOnController, v.flagOnController)
                MList.Update(_flagOnAnnotationSpace, v.flagOnAnnotationSpace)
                MList.Update(_lineOnController, v.lineOnController)
                MList.Update(_lineOnAnnotationSpace, v.lineOnAnnotationSpace)
                ResetMod.Update(_lineIsHovered,v.lineIsHovered)
                ResetMod.Update(_lineMarsDisplay,v.lineMarsDisplay)
                MMap.Update(_finishedLine, v.finishedLine)
                MList.Update(_dipAndStrikeOnController, v.dipAndStrikeOnController)
                MList.Update(_dipAndStrikeOnAnnotationSpace, v.dipAndStrikeOnAnnotationSpace)
                ResetMod.Update(_dipAndStrikeAngle,v.dipAndStrikeAngle)
                
        
        static member Create(__initial : Demo.Main.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : Demo.Main.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Demo.Main.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let text =
                { new Lens<Demo.Main.Model, System.String>() with
                    override x.Get(r) = r.text
                    override x.Set(r,v) = { r with text = v }
                    override x.Update(r,f) = { r with text = f r.text }
                }
            let vr =
                { new Lens<Demo.Main.Model, System.Boolean>() with
                    override x.Get(r) = r.vr
                    override x.Set(r,v) = { r with vr = v }
                    override x.Update(r,f) = { r with vr = f r.vr }
                }
            let cameraState =
                { new Lens<Demo.Main.Model, Aardvark.UI.Primitives.CameraControllerState>() with
                    override x.Get(r) = r.cameraState
                    override x.Set(r,v) = { r with cameraState = v }
                    override x.Update(r,f) = { r with cameraState = f r.cameraState }
                }
            let ControllerPosition =
                { new Lens<Demo.Main.Model, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.ControllerPosition
                    override x.Set(r,v) = { r with ControllerPosition = v }
                    override x.Update(r,f) = { r with ControllerPosition = f r.ControllerPosition }
                }
            let offsetToCenter =
                { new Lens<Demo.Main.Model, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.offsetToCenter
                    override x.Set(r,v) = { r with offsetToCenter = v }
                    override x.Update(r,f) = { r with offsetToCenter = f r.offsetToCenter }
                }
            let controllerInfos =
                { new Lens<Demo.Main.Model, Aardvark.Base.hmap<Demo.ControllerKind,Demo.ControllerInfo>>() with
                    override x.Get(r) = r.controllerInfos
                    override x.Set(r,v) = { r with controllerInfos = v }
                    override x.Update(r,f) = { r with controllerInfos = f r.controllerInfos }
                }
            let offsetControllerDistance =
                { new Lens<Demo.Main.Model, System.Double>() with
                    override x.Get(r) = r.offsetControllerDistance
                    override x.Set(r,v) = { r with offsetControllerDistance = v }
                    override x.Update(r,f) = { r with offsetControllerDistance = f r.offsetControllerDistance }
                }
            let patchHierarchies =
                { new Lens<Demo.Main.Model, Microsoft.FSharp.Collections.List<Aardvark.SceneGraph.Opc.PatchHierarchy>>() with
                    override x.Get(r) = r.patchHierarchies
                    override x.Set(r,v) = { r with patchHierarchies = v }
                    override x.Update(r,f) = { r with patchHierarchies = f r.patchHierarchies }
                }
            let boundingBox =
                { new Lens<Demo.Main.Model, Aardvark.Base.Box3d>() with
                    override x.Get(r) = r.boundingBox
                    override x.Set(r,v) = { r with boundingBox = v }
                    override x.Update(r,f) = { r with boundingBox = f r.boundingBox }
                }
            let rotationAxis =
                { new Lens<Demo.Main.Model, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.rotationAxis
                    override x.Set(r,v) = { r with rotationAxis = v }
                    override x.Update(r,f) = { r with rotationAxis = f r.rotationAxis }
                }
            let opcInfos =
                { new Lens<Demo.Main.Model, Aardvark.Base.hmap<Aardvark.Base.Box3d,OpcViewer.Base.Picking.OpcData>>() with
                    override x.Get(r) = r.opcInfos
                    override x.Set(r,v) = { r with opcInfos = v }
                    override x.Update(r,f) = { r with opcInfos = f r.opcInfos }
                }
            let opcAttributes =
                { new Lens<Demo.Main.Model, OpcViewer.Base.Attributes.AttributeModel>() with
                    override x.Get(r) = r.opcAttributes
                    override x.Set(r,v) = { r with opcAttributes = v }
                    override x.Update(r,f) = { r with opcAttributes = f r.opcAttributes }
                }
            let mainFrustum =
                { new Lens<Demo.Main.Model, Aardvark.Base.Frustum>() with
                    override x.Get(r) = r.mainFrustum
                    override x.Set(r,v) = { r with mainFrustum = v }
                    override x.Update(r,f) = { r with mainFrustum = f r.mainFrustum }
                }
            let rotateBox =
                { new Lens<Demo.Main.Model, System.Boolean>() with
                    override x.Get(r) = r.rotateBox
                    override x.Set(r,v) = { r with rotateBox = v }
                    override x.Update(r,f) = { r with rotateBox = f r.rotateBox }
                }
            let pickingModel =
                { new Lens<Demo.Main.Model, OpcViewer.Base.Picking.PickingModel>() with
                    override x.Get(r) = r.pickingModel
                    override x.Set(r,v) = { r with pickingModel = v }
                    override x.Update(r,f) = { r with pickingModel = f r.pickingModel }
                }
            let initWorkSpaceTrafo =
                { new Lens<Demo.Main.Model, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.initWorkSpaceTrafo
                    override x.Set(r,v) = { r with initWorkSpaceTrafo = v }
                    override x.Update(r,f) = { r with initWorkSpaceTrafo = f r.initWorkSpaceTrafo }
                }
            let workSpaceTrafo =
                { new Lens<Demo.Main.Model, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.workSpaceTrafo
                    override x.Set(r,v) = { r with workSpaceTrafo = v }
                    override x.Update(r,f) = { r with workSpaceTrafo = f r.workSpaceTrafo }
                }
            let opcSpaceTrafo =
                { new Lens<Demo.Main.Model, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.opcSpaceTrafo
                    override x.Set(r,v) = { r with opcSpaceTrafo = v }
                    override x.Update(r,f) = { r with opcSpaceTrafo = f r.opcSpaceTrafo }
                }
            let annotationSpaceTrafo =
                { new Lens<Demo.Main.Model, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.annotationSpaceTrafo
                    override x.Set(r,v) = { r with annotationSpaceTrafo = v }
                    override x.Update(r,f) = { r with annotationSpaceTrafo = f r.annotationSpaceTrafo }
                }
            let initOpcSpaceTrafo =
                { new Lens<Demo.Main.Model, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.initOpcSpaceTrafo
                    override x.Set(r,v) = { r with initOpcSpaceTrafo = v }
                    override x.Update(r,f) = { r with initOpcSpaceTrafo = f r.initOpcSpaceTrafo }
                }
            let initAnnotationSpaceTrafo =
                { new Lens<Demo.Main.Model, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.initAnnotationSpaceTrafo
                    override x.Set(r,v) = { r with initAnnotationSpaceTrafo = v }
                    override x.Update(r,f) = { r with initAnnotationSpaceTrafo = f r.initAnnotationSpaceTrafo }
                }
            let initControlTrafo =
                { new Lens<Demo.Main.Model, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.initControlTrafo
                    override x.Set(r,v) = { r with initControlTrafo = v }
                    override x.Update(r,f) = { r with initControlTrafo = f r.initControlTrafo }
                }
            let init2ControlTrafo =
                { new Lens<Demo.Main.Model, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.init2ControlTrafo
                    override x.Set(r,v) = { r with init2ControlTrafo = v }
                    override x.Update(r,f) = { r with init2ControlTrafo = f r.init2ControlTrafo }
                }
            let menuModel =
                { new Lens<Demo.Main.Model, Demo.Menu.MenuModel>() with
                    override x.Get(r) = r.menuModel
                    override x.Set(r,v) = { r with menuModel = v }
                    override x.Update(r,f) = { r with menuModel = f r.menuModel }
                }
            let drawingPoint =
                { new Lens<Demo.Main.Model, Aardvark.Base.plist<Demo.VisibleBox>>() with
                    override x.Get(r) = r.drawingPoint
                    override x.Set(r,v) = { r with drawingPoint = v }
                    override x.Update(r,f) = { r with drawingPoint = f r.drawingPoint }
                }
            let drawingLine =
                { new Lens<Demo.Main.Model, Aardvark.Base.Line3d[]>() with
                    override x.Get(r) = r.drawingLine
                    override x.Set(r,v) = { r with drawingLine = v }
                    override x.Update(r,f) = { r with drawingLine = f r.drawingLine }
                }
            let currentlyDrawing =
                { new Lens<Demo.Main.Model, Microsoft.FSharp.Core.Option<Demo.Main.Polygon>>() with
                    override x.Get(r) = r.currentlyDrawing
                    override x.Set(r,v) = { r with currentlyDrawing = v }
                    override x.Update(r,f) = { r with currentlyDrawing = f r.currentlyDrawing }
                }
            let finishedDrawings =
                { new Lens<Demo.Main.Model, Aardvark.Base.hmap<System.String,Demo.Main.Polygon>>() with
                    override x.Get(r) = r.finishedDrawings
                    override x.Set(r,v) = { r with finishedDrawings = v }
                    override x.Update(r,f) = { r with finishedDrawings = f r.finishedDrawings }
                }
            let flagOnController =
                { new Lens<Demo.Main.Model, Aardvark.Base.plist<Demo.VisibleBox>>() with
                    override x.Get(r) = r.flagOnController
                    override x.Set(r,v) = { r with flagOnController = v }
                    override x.Update(r,f) = { r with flagOnController = f r.flagOnController }
                }
            let flagOnAnnotationSpace =
                { new Lens<Demo.Main.Model, Aardvark.Base.plist<Demo.VisibleBox>>() with
                    override x.Get(r) = r.flagOnAnnotationSpace
                    override x.Set(r,v) = { r with flagOnAnnotationSpace = v }
                    override x.Update(r,f) = { r with flagOnAnnotationSpace = f r.flagOnAnnotationSpace }
                }
            let lineOnController =
                { new Lens<Demo.Main.Model, Aardvark.Base.plist<Demo.VisibleSphere>>() with
                    override x.Get(r) = r.lineOnController
                    override x.Set(r,v) = { r with lineOnController = v }
                    override x.Update(r,f) = { r with lineOnController = f r.lineOnController }
                }
            let lineOnAnnotationSpace =
                { new Lens<Demo.Main.Model, Aardvark.Base.plist<Demo.VisibleSphere>>() with
                    override x.Get(r) = r.lineOnAnnotationSpace
                    override x.Set(r,v) = { r with lineOnAnnotationSpace = v }
                    override x.Update(r,f) = { r with lineOnAnnotationSpace = f r.lineOnAnnotationSpace }
                }
            let lineIsHovered =
                { new Lens<Demo.Main.Model, System.Boolean>() with
                    override x.Get(r) = r.lineIsHovered
                    override x.Set(r,v) = { r with lineIsHovered = v }
                    override x.Update(r,f) = { r with lineIsHovered = f r.lineIsHovered }
                }
            let lineMarsDisplay =
                { new Lens<Demo.Main.Model, Aardvark.Base.Line3d[]>() with
                    override x.Get(r) = r.lineMarsDisplay
                    override x.Set(r,v) = { r with lineMarsDisplay = v }
                    override x.Update(r,f) = { r with lineMarsDisplay = f r.lineMarsDisplay }
                }
            let finishedLine =
                { new Lens<Demo.Main.Model, Aardvark.Base.hmap<System.String,Demo.Main.FinishedLine>>() with
                    override x.Get(r) = r.finishedLine
                    override x.Set(r,v) = { r with finishedLine = v }
                    override x.Update(r,f) = { r with finishedLine = f r.finishedLine }
                }
            let dipAndStrikeOnController =
                { new Lens<Demo.Main.Model, Aardvark.Base.plist<Demo.VisibleCylinder>>() with
                    override x.Get(r) = r.dipAndStrikeOnController
                    override x.Set(r,v) = { r with dipAndStrikeOnController = v }
                    override x.Update(r,f) = { r with dipAndStrikeOnController = f r.dipAndStrikeOnController }
                }
            let dipAndStrikeOnAnnotationSpace =
                { new Lens<Demo.Main.Model, Aardvark.Base.plist<Demo.VisibleCylinder>>() with
                    override x.Get(r) = r.dipAndStrikeOnAnnotationSpace
                    override x.Set(r,v) = { r with dipAndStrikeOnAnnotationSpace = v }
                    override x.Update(r,f) = { r with dipAndStrikeOnAnnotationSpace = f r.dipAndStrikeOnAnnotationSpace }
                }
            let dipAndStrikeAngle =
                { new Lens<Demo.Main.Model, System.Double>() with
                    override x.Get(r) = r.dipAndStrikeAngle
                    override x.Set(r,v) = { r with dipAndStrikeAngle = v }
                    override x.Update(r,f) = { r with dipAndStrikeAngle = f r.dipAndStrikeAngle }
                }

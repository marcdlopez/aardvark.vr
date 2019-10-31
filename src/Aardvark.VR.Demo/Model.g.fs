namespace Demo

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Demo

[<AutoOpen>]
module Mutable =

    
    
    type MVisibleBox(__initial : Demo.VisibleBox) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Demo.VisibleBox> = Aardvark.Base.Incremental.EqModRef<Demo.VisibleBox>(__initial) :> Aardvark.Base.Incremental.IModRef<Demo.VisibleBox>
        let _geometry = ResetMod.Create(__initial.geometry)
        let _color = ResetMod.Create(__initial.color)
        let _trafo = ResetMod.Create(__initial.trafo)
        let _id = ResetMod.Create(__initial.id)
        
        member x.geometry = _geometry :> IMod<_>
        member x.color = _color :> IMod<_>
        member x.trafo = _trafo :> IMod<_>
        member x.id = _id :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Demo.VisibleBox) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_geometry,v.geometry)
                ResetMod.Update(_color,v.color)
                ResetMod.Update(_trafo,v.trafo)
                _id.Update(v.id)
                
        
        static member Create(__initial : Demo.VisibleBox) : MVisibleBox = MVisibleBox(__initial)
        static member Update(m : MVisibleBox, v : Demo.VisibleBox) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Demo.VisibleBox> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module VisibleBox =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let geometry =
                { new Lens<Demo.VisibleBox, Aardvark.Base.Box3d>() with
                    override x.Get(r) = r.geometry
                    override x.Set(r,v) = { r with geometry = v }
                    override x.Update(r,f) = { r with geometry = f r.geometry }
                }
            let color =
                { new Lens<Demo.VisibleBox, Aardvark.Base.C4b>() with
                    override x.Get(r) = r.color
                    override x.Set(r,v) = { r with color = v }
                    override x.Update(r,f) = { r with color = f r.color }
                }
            let trafo =
                { new Lens<Demo.VisibleBox, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.trafo
                    override x.Set(r,v) = { r with trafo = v }
                    override x.Update(r,f) = { r with trafo = f r.trafo }
                }
            let id =
                { new Lens<Demo.VisibleBox, System.String>() with
                    override x.Get(r) = r.id
                    override x.Set(r,v) = { r with id = v }
                    override x.Update(r,f) = { r with id = f r.id }
                }
    
    
    type MLine(__initial : Demo.Line) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Demo.Line> = Aardvark.Base.Incremental.EqModRef<Demo.Line>(__initial) :> Aardvark.Base.Incremental.IModRef<Demo.Line>
        let _line = ResetMod.Create(__initial.line)
        let _color = ResetMod.Create(__initial.color)
        
        member x.line = _line :> IMod<_>
        member x.color = _color :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Demo.Line) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_line,v.line)
                ResetMod.Update(_color,v.color)
                
        
        static member Create(__initial : Demo.Line) : MLine = MLine(__initial)
        static member Update(m : MLine, v : Demo.Line) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Demo.Line> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Line =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let line =
                { new Lens<Demo.Line, Aardvark.Base.Line3d>() with
                    override x.Get(r) = r.line
                    override x.Set(r,v) = { r with line = v }
                    override x.Update(r,f) = { r with line = f r.line }
                }
            let color =
                { new Lens<Demo.Line, Aardvark.Base.C4b>() with
                    override x.Get(r) = r.color
                    override x.Set(r,v) = { r with color = v }
                    override x.Update(r,f) = { r with color = f r.color }
                }
    
    
    type MModel(__initial : Demo.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Demo.Model> = Aardvark.Base.Incremental.EqModRef<Demo.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<Demo.Model>
        let _text = ResetMod.Create(__initial.text)
        let _vr = ResetMod.Create(__initial.vr)
        let _mainMenuBoxes = MList.Create(__initial.mainMenuBoxes, (fun v -> MVisibleBox.Create(v)), (fun (m,v) -> MVisibleBox.Update(m, v)), (fun v -> v))
        let _boxHovered = MOption.Create(__initial.boxHovered)
        let _subMenuBoxes = MList.Create(__initial.subMenuBoxes, (fun v -> MVisibleBox.Create(v)), (fun (m,v) -> MVisibleBox.Update(m, v)), (fun v -> v))
        let _cameraState = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.cameraState)
        let _ControllerPosition = ResetMod.Create(__initial.ControllerPosition)
        let _offsetToCenter = ResetMod.Create(__initial.offsetToCenter)
        let _controllerInfos = MMap.Create(__initial.controllerInfos, (fun v -> MControllerInfo.Create(v)), (fun (m,v) -> MControllerInfo.Update(m, v)), (fun v -> v))
        let _offsetControllerDistance = ResetMod.Create(__initial.offsetControllerDistance)
        let _boundingBox = ResetMod.Create(__initial.boundingBox)
        let _rotationAxis = ResetMod.Create(__initial.rotationAxis)
        let _opcInfos = MMap.Create(__initial.opcInfos, (fun v -> OpcViewer.Base.Picking.Mutable.MOpcData.Create(v)), (fun (m,v) -> OpcViewer.Base.Picking.Mutable.MOpcData.Update(m, v)), (fun v -> v))
        let _opcAttributes = OpcViewer.Base.Attributes.Mutable.MAttributeModel.Create(__initial.opcAttributes)
        let _mainFrustum = ResetMod.Create(__initial.mainFrustum)
        let _rotateBox = ResetMod.Create(__initial.rotateBox)
        let _pickingModel = OpcViewer.Base.Picking.Mutable.MPickingModel.Create(__initial.pickingModel)
        let _globalTrafo = ResetMod.Create(__initial.globalTrafo)
        let _initGlobalTrafo = ResetMod.Create(__initial.initGlobalTrafo)
        let _initControlTrafo = ResetMod.Create(__initial.initControlTrafo)
        let _init2ControlTrafo = ResetMod.Create(__initial.init2ControlTrafo)
        let _menu = ResetMod.Create(__initial.menu)
        let _annotationMenu = ResetMod.Create(__initial.annotationMenu)
        let _initialMenuState = ResetMod.Create(__initial.initialMenuState)
        let _menuButtonPressed = ResetMod.Create(__initial.menuButtonPressed)
        let _initialMenuPosition = Aardvark.Vr.Mutable.MPose.Create(__initial.initialMenuPosition)
        let _initialMenuPositionBool = ResetMod.Create(__initial.initialMenuPositionBool)
        let _controllerMenuSelector = ResetMod.Create(__initial.controllerMenuSelector)
        let _drawingPoint = MList.Create(__initial.drawingPoint, (fun v -> MVisibleBox.Create(v)), (fun (m,v) -> MVisibleBox.Update(m, v)), (fun v -> v))
        
        member x.text = _text :> IMod<_>
        member x.vr = _vr :> IMod<_>
        member x.mainMenuBoxes = _mainMenuBoxes :> alist<_>
        member x.boxHovered = _boxHovered :> IMod<_>
        member x.subMenuBoxes = _subMenuBoxes :> alist<_>
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
        member x.globalTrafo = _globalTrafo :> IMod<_>
        member x.initGlobalTrafo = _initGlobalTrafo :> IMod<_>
        member x.initControlTrafo = _initControlTrafo :> IMod<_>
        member x.init2ControlTrafo = _init2ControlTrafo :> IMod<_>
        member x.menu = _menu :> IMod<_>
        member x.annotationMenu = _annotationMenu :> IMod<_>
        member x.initialMenuState = _initialMenuState :> IMod<_>
        member x.menuButtonPressed = _menuButtonPressed :> IMod<_>
        member x.initialMenuPosition = _initialMenuPosition
        member x.initialMenuPositionBool = _initialMenuPositionBool :> IMod<_>
        member x.controllerMenuSelector = _controllerMenuSelector :> IMod<_>
        member x.drawingPoint = _drawingPoint :> alist<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Demo.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_text,v.text)
                ResetMod.Update(_vr,v.vr)
                MList.Update(_mainMenuBoxes, v.mainMenuBoxes)
                MOption.Update(_boxHovered, v.boxHovered)
                MList.Update(_subMenuBoxes, v.subMenuBoxes)
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
                ResetMod.Update(_globalTrafo,v.globalTrafo)
                ResetMod.Update(_initGlobalTrafo,v.initGlobalTrafo)
                ResetMod.Update(_initControlTrafo,v.initControlTrafo)
                ResetMod.Update(_init2ControlTrafo,v.init2ControlTrafo)
                ResetMod.Update(_menu,v.menu)
                ResetMod.Update(_annotationMenu,v.annotationMenu)
                ResetMod.Update(_initialMenuState,v.initialMenuState)
                ResetMod.Update(_menuButtonPressed,v.menuButtonPressed)
                Aardvark.Vr.Mutable.MPose.Update(_initialMenuPosition, v.initialMenuPosition)
                ResetMod.Update(_initialMenuPositionBool,v.initialMenuPositionBool)
                ResetMod.Update(_controllerMenuSelector,v.controllerMenuSelector)
                MList.Update(_drawingPoint, v.drawingPoint)
                
        
        static member Create(__initial : Demo.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : Demo.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Demo.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let text =
                { new Lens<Demo.Model, System.String>() with
                    override x.Get(r) = r.text
                    override x.Set(r,v) = { r with text = v }
                    override x.Update(r,f) = { r with text = f r.text }
                }
            let vr =
                { new Lens<Demo.Model, System.Boolean>() with
                    override x.Get(r) = r.vr
                    override x.Set(r,v) = { r with vr = v }
                    override x.Update(r,f) = { r with vr = f r.vr }
                }
            let mainMenuBoxes =
                { new Lens<Demo.Model, Aardvark.Base.plist<Demo.VisibleBox>>() with
                    override x.Get(r) = r.mainMenuBoxes
                    override x.Set(r,v) = { r with mainMenuBoxes = v }
                    override x.Update(r,f) = { r with mainMenuBoxes = f r.mainMenuBoxes }
                }
            let boxHovered =
                { new Lens<Demo.Model, Microsoft.FSharp.Core.Option<System.String>>() with
                    override x.Get(r) = r.boxHovered
                    override x.Set(r,v) = { r with boxHovered = v }
                    override x.Update(r,f) = { r with boxHovered = f r.boxHovered }
                }
            let subMenuBoxes =
                { new Lens<Demo.Model, Aardvark.Base.plist<Demo.VisibleBox>>() with
                    override x.Get(r) = r.subMenuBoxes
                    override x.Set(r,v) = { r with subMenuBoxes = v }
                    override x.Update(r,f) = { r with subMenuBoxes = f r.subMenuBoxes }
                }
            let cameraState =
                { new Lens<Demo.Model, Aardvark.UI.Primitives.CameraControllerState>() with
                    override x.Get(r) = r.cameraState
                    override x.Set(r,v) = { r with cameraState = v }
                    override x.Update(r,f) = { r with cameraState = f r.cameraState }
                }
            let ControllerPosition =
                { new Lens<Demo.Model, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.ControllerPosition
                    override x.Set(r,v) = { r with ControllerPosition = v }
                    override x.Update(r,f) = { r with ControllerPosition = f r.ControllerPosition }
                }
            let offsetToCenter =
                { new Lens<Demo.Model, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.offsetToCenter
                    override x.Set(r,v) = { r with offsetToCenter = v }
                    override x.Update(r,f) = { r with offsetToCenter = f r.offsetToCenter }
                }
            let controllerInfos =
                { new Lens<Demo.Model, Aardvark.Base.hmap<Demo.ControllerKind,Demo.ControllerInfo>>() with
                    override x.Get(r) = r.controllerInfos
                    override x.Set(r,v) = { r with controllerInfos = v }
                    override x.Update(r,f) = { r with controllerInfos = f r.controllerInfos }
                }
            let offsetControllerDistance =
                { new Lens<Demo.Model, System.Double>() with
                    override x.Get(r) = r.offsetControllerDistance
                    override x.Set(r,v) = { r with offsetControllerDistance = v }
                    override x.Update(r,f) = { r with offsetControllerDistance = f r.offsetControllerDistance }
                }
            let patchHierarchies =
                { new Lens<Demo.Model, Microsoft.FSharp.Collections.List<Aardvark.SceneGraph.Opc.PatchHierarchy>>() with
                    override x.Get(r) = r.patchHierarchies
                    override x.Set(r,v) = { r with patchHierarchies = v }
                    override x.Update(r,f) = { r with patchHierarchies = f r.patchHierarchies }
                }
            let boundingBox =
                { new Lens<Demo.Model, Aardvark.Base.Box3d>() with
                    override x.Get(r) = r.boundingBox
                    override x.Set(r,v) = { r with boundingBox = v }
                    override x.Update(r,f) = { r with boundingBox = f r.boundingBox }
                }
            let rotationAxis =
                { new Lens<Demo.Model, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.rotationAxis
                    override x.Set(r,v) = { r with rotationAxis = v }
                    override x.Update(r,f) = { r with rotationAxis = f r.rotationAxis }
                }
            let opcInfos =
                { new Lens<Demo.Model, Aardvark.Base.hmap<Aardvark.Base.Box3d,OpcViewer.Base.Picking.OpcData>>() with
                    override x.Get(r) = r.opcInfos
                    override x.Set(r,v) = { r with opcInfos = v }
                    override x.Update(r,f) = { r with opcInfos = f r.opcInfos }
                }
            let opcAttributes =
                { new Lens<Demo.Model, OpcViewer.Base.Attributes.AttributeModel>() with
                    override x.Get(r) = r.opcAttributes
                    override x.Set(r,v) = { r with opcAttributes = v }
                    override x.Update(r,f) = { r with opcAttributes = f r.opcAttributes }
                }
            let mainFrustum =
                { new Lens<Demo.Model, Aardvark.Base.Frustum>() with
                    override x.Get(r) = r.mainFrustum
                    override x.Set(r,v) = { r with mainFrustum = v }
                    override x.Update(r,f) = { r with mainFrustum = f r.mainFrustum }
                }
            let rotateBox =
                { new Lens<Demo.Model, System.Boolean>() with
                    override x.Get(r) = r.rotateBox
                    override x.Set(r,v) = { r with rotateBox = v }
                    override x.Update(r,f) = { r with rotateBox = f r.rotateBox }
                }
            let pickingModel =
                { new Lens<Demo.Model, OpcViewer.Base.Picking.PickingModel>() with
                    override x.Get(r) = r.pickingModel
                    override x.Set(r,v) = { r with pickingModel = v }
                    override x.Update(r,f) = { r with pickingModel = f r.pickingModel }
                }
            let globalTrafo =
                { new Lens<Demo.Model, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.globalTrafo
                    override x.Set(r,v) = { r with globalTrafo = v }
                    override x.Update(r,f) = { r with globalTrafo = f r.globalTrafo }
                }
            let initGlobalTrafo =
                { new Lens<Demo.Model, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.initGlobalTrafo
                    override x.Set(r,v) = { r with initGlobalTrafo = v }
                    override x.Update(r,f) = { r with initGlobalTrafo = f r.initGlobalTrafo }
                }
            let initControlTrafo =
                { new Lens<Demo.Model, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.initControlTrafo
                    override x.Set(r,v) = { r with initControlTrafo = v }
                    override x.Update(r,f) = { r with initControlTrafo = f r.initControlTrafo }
                }
            let init2ControlTrafo =
                { new Lens<Demo.Model, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.init2ControlTrafo
                    override x.Set(r,v) = { r with init2ControlTrafo = v }
                    override x.Update(r,f) = { r with init2ControlTrafo = f r.init2ControlTrafo }
                }
            let menu =
                { new Lens<Demo.Model, Demo.MenuState>() with
                    override x.Get(r) = r.menu
                    override x.Set(r,v) = { r with menu = v }
                    override x.Update(r,f) = { r with menu = f r.menu }
                }
            let annotationMenu =
                { new Lens<Demo.Model, Demo.subMenuState>() with
                    override x.Get(r) = r.annotationMenu
                    override x.Set(r,v) = { r with annotationMenu = v }
                    override x.Update(r,f) = { r with annotationMenu = f r.annotationMenu }
                }
            let initialMenuState =
                { new Lens<Demo.Model, Demo.MenuState>() with
                    override x.Get(r) = r.initialMenuState
                    override x.Set(r,v) = { r with initialMenuState = v }
                    override x.Update(r,f) = { r with initialMenuState = f r.initialMenuState }
                }
            let menuButtonPressed =
                { new Lens<Demo.Model, System.Boolean>() with
                    override x.Get(r) = r.menuButtonPressed
                    override x.Set(r,v) = { r with menuButtonPressed = v }
                    override x.Update(r,f) = { r with menuButtonPressed = f r.menuButtonPressed }
                }
            let initialMenuPosition =
                { new Lens<Demo.Model, Aardvark.Vr.Pose>() with
                    override x.Get(r) = r.initialMenuPosition
                    override x.Set(r,v) = { r with initialMenuPosition = v }
                    override x.Update(r,f) = { r with initialMenuPosition = f r.initialMenuPosition }
                }
            let initialMenuPositionBool =
                { new Lens<Demo.Model, System.Boolean>() with
                    override x.Get(r) = r.initialMenuPositionBool
                    override x.Set(r,v) = { r with initialMenuPositionBool = v }
                    override x.Update(r,f) = { r with initialMenuPositionBool = f r.initialMenuPositionBool }
                }
            let controllerMenuSelector =
                { new Lens<Demo.Model, Demo.ControllerKind>() with
                    override x.Get(r) = r.controllerMenuSelector
                    override x.Set(r,v) = { r with controllerMenuSelector = v }
                    override x.Update(r,f) = { r with controllerMenuSelector = f r.controllerMenuSelector }
                }
            let drawingPoint =
                { new Lens<Demo.Model, Aardvark.Base.plist<Demo.VisibleBox>>() with
                    override x.Get(r) = r.drawingPoint
                    override x.Set(r,v) = { r with drawingPoint = v }
                    override x.Update(r,f) = { r with drawingPoint = f r.drawingPoint }
                }

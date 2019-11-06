namespace Demo.Main

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Demo.Main

[<AutoOpen>]
module Mutable =

    
    
    type MLine(__initial : Demo.Main.Line) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Demo.Main.Line> = Aardvark.Base.Incremental.EqModRef<Demo.Main.Line>(__initial) :> Aardvark.Base.Incremental.IModRef<Demo.Main.Line>
        let _line = ResetMod.Create(__initial.line)
        let _color = ResetMod.Create(__initial.color)
        
        member x.line = _line :> IMod<_>
        member x.color = _color :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Demo.Main.Line) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_line,v.line)
                ResetMod.Update(_color,v.color)
                
        
        static member Create(__initial : Demo.Main.Line) : MLine = MLine(__initial)
        static member Update(m : MLine, v : Demo.Main.Line) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Demo.Main.Line> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Line =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let line =
                { new Lens<Demo.Main.Line, Aardvark.Base.Line3d>() with
                    override x.Get(r) = r.line
                    override x.Set(r,v) = { r with line = v }
                    override x.Update(r,f) = { r with line = f r.line }
                }
            let color =
                { new Lens<Demo.Main.Line, Aardvark.Base.C4b>() with
                    override x.Get(r) = r.color
                    override x.Set(r,v) = { r with color = v }
                    override x.Update(r,f) = { r with color = f r.color }
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
        let _globalTrafo = ResetMod.Create(__initial.globalTrafo)
        let _initGlobalTrafo = ResetMod.Create(__initial.initGlobalTrafo)
        let _initControlTrafo = ResetMod.Create(__initial.initControlTrafo)
        let _init2ControlTrafo = ResetMod.Create(__initial.init2ControlTrafo)
        let _menuModel = Demo.Menu.Mutable.MMenuModel.Create(__initial.menuModel)
        let _drawingPoint = MList.Create(__initial.drawingPoint, (fun v -> Demo.Mutable.MVisibleBox.Create(v)), (fun (m,v) -> Demo.Mutable.MVisibleBox.Update(m, v)), (fun v -> v))
        let _flagOnController = MList.Create(__initial.flagOnController, (fun v -> Demo.Mutable.MVisibleBox.Create(v)), (fun (m,v) -> Demo.Mutable.MVisibleBox.Update(m, v)), (fun v -> v))
        let _flagOnMars = MList.Create(__initial.flagOnMars, (fun v -> Demo.Mutable.MVisibleBox.Create(v)), (fun (m,v) -> Demo.Mutable.MVisibleBox.Update(m, v)), (fun v -> v))
        
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
        member x.globalTrafo = _globalTrafo :> IMod<_>
        member x.initGlobalTrafo = _initGlobalTrafo :> IMod<_>
        member x.initControlTrafo = _initControlTrafo :> IMod<_>
        member x.init2ControlTrafo = _init2ControlTrafo :> IMod<_>
        member x.menuModel = _menuModel
        member x.drawingPoint = _drawingPoint :> alist<_>
        member x.flagOnController = _flagOnController :> alist<_>
        member x.flagOnMars = _flagOnMars :> alist<_>
        
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
                ResetMod.Update(_globalTrafo,v.globalTrafo)
                ResetMod.Update(_initGlobalTrafo,v.initGlobalTrafo)
                ResetMod.Update(_initControlTrafo,v.initControlTrafo)
                ResetMod.Update(_init2ControlTrafo,v.init2ControlTrafo)
                Demo.Menu.Mutable.MMenuModel.Update(_menuModel, v.menuModel)
                MList.Update(_drawingPoint, v.drawingPoint)
                MList.Update(_flagOnController, v.flagOnController)
                MList.Update(_flagOnMars, v.flagOnMars)
                
        
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
            let globalTrafo =
                { new Lens<Demo.Main.Model, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.globalTrafo
                    override x.Set(r,v) = { r with globalTrafo = v }
                    override x.Update(r,f) = { r with globalTrafo = f r.globalTrafo }
                }
            let initGlobalTrafo =
                { new Lens<Demo.Main.Model, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.initGlobalTrafo
                    override x.Set(r,v) = { r with initGlobalTrafo = v }
                    override x.Update(r,f) = { r with initGlobalTrafo = f r.initGlobalTrafo }
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
            let flagOnController =
                { new Lens<Demo.Main.Model, Aardvark.Base.plist<Demo.VisibleBox>>() with
                    override x.Get(r) = r.flagOnController
                    override x.Set(r,v) = { r with flagOnController = v }
                    override x.Update(r,f) = { r with flagOnController = f r.flagOnController }
                }
            let flagOnMars =
                { new Lens<Demo.Main.Model, Aardvark.Base.plist<Demo.VisibleBox>>() with
                    override x.Get(r) = r.flagOnMars
                    override x.Set(r,v) = { r with flagOnMars = v }
                    override x.Update(r,f) = { r with flagOnMars = f r.flagOnMars }
                }

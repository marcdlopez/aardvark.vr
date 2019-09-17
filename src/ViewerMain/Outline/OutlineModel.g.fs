namespace OpcOutlineTest

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open OpcOutlineTest

[<AutoOpen>]
module Mutable =

    
    
    type MOutlineModel(__initial : OpcOutlineTest.OutlineModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<OpcOutlineTest.OutlineModel> = Aardvark.Base.Incremental.EqModRef<OpcOutlineTest.OutlineModel>(__initial) :> Aardvark.Base.Incremental.IModRef<OpcOutlineTest.OutlineModel>
        let _cameraState = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.cameraState)
        let _fillMode = ResetMod.Create(__initial.fillMode)
        let _boxes = ResetMod.Create(__initial.boxes)
        let _opcInfos = MMap.Create(__initial.opcInfos, (fun v -> OpcViewer.Base.Picking.Mutable.MOpcData.Create(v)), (fun (m,v) -> OpcViewer.Base.Picking.Mutable.MOpcData.Update(m, v)), (fun v -> v))
        let _threads = ResetMod.Create(__initial.threads)
        let _dockConfig = ResetMod.Create(__initial.dockConfig)
        let _lineThickness = Aardvark.UI.Mutable.MNumericInput.Create(__initial.lineThickness)
        let _useOutlines = ResetMod.Create(__initial.useOutlines)
        
        member x.cameraState = _cameraState
        member x.fillMode = _fillMode :> IMod<_>
        member x.patchHierarchies = __current.Value.patchHierarchies
        member x.boxes = _boxes :> IMod<_>
        member x.opcInfos = _opcInfos :> amap<_,_>
        member x.threads = _threads :> IMod<_>
        member x.dockConfig = _dockConfig :> IMod<_>
        member x.lineThickness = _lineThickness
        member x.useOutlines = _useOutlines :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : OpcOutlineTest.OutlineModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                Aardvark.UI.Primitives.Mutable.MCameraControllerState.Update(_cameraState, v.cameraState)
                ResetMod.Update(_fillMode,v.fillMode)
                ResetMod.Update(_boxes,v.boxes)
                MMap.Update(_opcInfos, v.opcInfos)
                ResetMod.Update(_threads,v.threads)
                ResetMod.Update(_dockConfig,v.dockConfig)
                Aardvark.UI.Mutable.MNumericInput.Update(_lineThickness, v.lineThickness)
                ResetMod.Update(_useOutlines,v.useOutlines)
                
        
        static member Create(__initial : OpcOutlineTest.OutlineModel) : MOutlineModel = MOutlineModel(__initial)
        static member Update(m : MOutlineModel, v : OpcOutlineTest.OutlineModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<OpcOutlineTest.OutlineModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module OutlineModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let cameraState =
                { new Lens<OpcOutlineTest.OutlineModel, Aardvark.UI.Primitives.CameraControllerState>() with
                    override x.Get(r) = r.cameraState
                    override x.Set(r,v) = { r with cameraState = v }
                    override x.Update(r,f) = { r with cameraState = f r.cameraState }
                }
            let fillMode =
                { new Lens<OpcOutlineTest.OutlineModel, Aardvark.Base.Rendering.FillMode>() with
                    override x.Get(r) = r.fillMode
                    override x.Set(r,v) = { r with fillMode = v }
                    override x.Update(r,f) = { r with fillMode = f r.fillMode }
                }
            let patchHierarchies =
                { new Lens<OpcOutlineTest.OutlineModel, Microsoft.FSharp.Collections.List<Aardvark.SceneGraph.Opc.PatchHierarchy>>() with
                    override x.Get(r) = r.patchHierarchies
                    override x.Set(r,v) = { r with patchHierarchies = v }
                    override x.Update(r,f) = { r with patchHierarchies = f r.patchHierarchies }
                }
            let boxes =
                { new Lens<OpcOutlineTest.OutlineModel, Microsoft.FSharp.Collections.List<Aardvark.Base.Box3d>>() with
                    override x.Get(r) = r.boxes
                    override x.Set(r,v) = { r with boxes = v }
                    override x.Update(r,f) = { r with boxes = f r.boxes }
                }
            let opcInfos =
                { new Lens<OpcOutlineTest.OutlineModel, Aardvark.Base.hmap<Aardvark.Base.Box3d,OpcViewer.Base.Picking.OpcData>>() with
                    override x.Get(r) = r.opcInfos
                    override x.Set(r,v) = { r with opcInfos = v }
                    override x.Update(r,f) = { r with opcInfos = f r.opcInfos }
                }
            let threads =
                { new Lens<OpcOutlineTest.OutlineModel, Aardvark.Base.Incremental.ThreadPool<OpcOutlineTest.OutlineMessage>>() with
                    override x.Get(r) = r.threads
                    override x.Set(r,v) = { r with threads = v }
                    override x.Update(r,f) = { r with threads = f r.threads }
                }
            let dockConfig =
                { new Lens<OpcOutlineTest.OutlineModel, Aardvark.UI.Primitives.DockConfig>() with
                    override x.Get(r) = r.dockConfig
                    override x.Set(r,v) = { r with dockConfig = v }
                    override x.Update(r,f) = { r with dockConfig = f r.dockConfig }
                }
            let lineThickness =
                { new Lens<OpcOutlineTest.OutlineModel, Aardvark.UI.NumericInput>() with
                    override x.Get(r) = r.lineThickness
                    override x.Set(r,v) = { r with lineThickness = v }
                    override x.Update(r,f) = { r with lineThickness = f r.lineThickness }
                }
            let useOutlines =
                { new Lens<OpcOutlineTest.OutlineModel, System.Boolean>() with
                    override x.Get(r) = r.useOutlines
                    override x.Set(r,v) = { r with useOutlines = v }
                    override x.Update(r,f) = { r with useOutlines = f r.useOutlines }
                }

namespace OpcViewer.Base.Picking

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open OpcViewer.Base.Picking

[<AutoOpen>]
module Mutable =

    
    
    type MAxisPointInfo(__initial : OpcViewer.Base.Picking.AxisPointInfo) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<OpcViewer.Base.Picking.AxisPointInfo> = Aardvark.Base.Incremental.EqModRef<OpcViewer.Base.Picking.AxisPointInfo>(__initial) :> Aardvark.Base.Incremental.IModRef<OpcViewer.Base.Picking.AxisPointInfo>
        let _pointsOnAxis = MList.Create(__initial.pointsOnAxis)
        let _midPoint = ResetMod.Create(__initial.midPoint)
        
        member x.pointsOnAxis = _pointsOnAxis :> alist<_>
        member x.midPoint = _midPoint :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : OpcViewer.Base.Picking.AxisPointInfo) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MList.Update(_pointsOnAxis, v.pointsOnAxis)
                ResetMod.Update(_midPoint,v.midPoint)
                
        
        static member Create(__initial : OpcViewer.Base.Picking.AxisPointInfo) : MAxisPointInfo = MAxisPointInfo(__initial)
        static member Update(m : MAxisPointInfo, v : OpcViewer.Base.Picking.AxisPointInfo) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<OpcViewer.Base.Picking.AxisPointInfo> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module AxisPointInfo =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let pointsOnAxis =
                { new Lens<OpcViewer.Base.Picking.AxisPointInfo, Aardvark.Base.plist<Aardvark.Base.V3d>>() with
                    override x.Get(r) = r.pointsOnAxis
                    override x.Set(r,v) = { r with pointsOnAxis = v }
                    override x.Update(r,f) = { r with pointsOnAxis = f r.pointsOnAxis }
                }
            let midPoint =
                { new Lens<OpcViewer.Base.Picking.AxisPointInfo, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.midPoint
                    override x.Set(r,v) = { r with midPoint = v }
                    override x.Update(r,f) = { r with midPoint = f r.midPoint }
                }
    
    
    type MOpcData(__initial : OpcViewer.Base.Picking.OpcData) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<OpcViewer.Base.Picking.OpcData> = Aardvark.Base.Incremental.EqModRef<OpcViewer.Base.Picking.OpcData>(__initial) :> Aardvark.Base.Incremental.IModRef<OpcViewer.Base.Picking.OpcData>
        let _kdTree = MMap.Create(__initial.kdTree)
        let _neighborMap = MMap.Create(__initial.neighborMap)
        let _localBB = ResetMod.Create(__initial.localBB)
        let _globalBB = ResetMod.Create(__initial.globalBB)
        
        member x.patchHierarchy = __current.Value.patchHierarchy
        member x.kdTree = _kdTree :> amap<_,_>
        member x.neighborMap = _neighborMap :> amap<_,_>
        member x.localBB = _localBB :> IMod<_>
        member x.globalBB = _globalBB :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : OpcViewer.Base.Picking.OpcData) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MMap.Update(_kdTree, v.kdTree)
                MMap.Update(_neighborMap, v.neighborMap)
                ResetMod.Update(_localBB,v.localBB)
                ResetMod.Update(_globalBB,v.globalBB)
                
        
        static member Create(__initial : OpcViewer.Base.Picking.OpcData) : MOpcData = MOpcData(__initial)
        static member Update(m : MOpcData, v : OpcViewer.Base.Picking.OpcData) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<OpcViewer.Base.Picking.OpcData> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module OpcData =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let patchHierarchy =
                { new Lens<OpcViewer.Base.Picking.OpcData, Aardvark.SceneGraph.Opc.PatchHierarchy>() with
                    override x.Get(r) = r.patchHierarchy
                    override x.Set(r,v) = { r with patchHierarchy = v }
                    override x.Update(r,f) = { r with patchHierarchy = f r.patchHierarchy }
                }
            let kdTree =
                { new Lens<OpcViewer.Base.Picking.OpcData, Aardvark.Base.hmap<Aardvark.Base.Box3d,Aardvark.VRVis.Opc.KdTrees.Level0KdTree>>() with
                    override x.Get(r) = r.kdTree
                    override x.Set(r,v) = { r with kdTree = v }
                    override x.Update(r,f) = { r with kdTree = f r.kdTree }
                }
            let neighborMap =
                { new Lens<OpcViewer.Base.Picking.OpcData, Aardvark.Base.hmap<Aardvark.Base.Box3d,OpcViewer.Base.Picking.BoxNeighbors>>() with
                    override x.Get(r) = r.neighborMap
                    override x.Set(r,v) = { r with neighborMap = v }
                    override x.Update(r,f) = { r with neighborMap = f r.neighborMap }
                }
            let localBB =
                { new Lens<OpcViewer.Base.Picking.OpcData, Aardvark.Base.Box3d>() with
                    override x.Get(r) = r.localBB
                    override x.Set(r,v) = { r with localBB = v }
                    override x.Update(r,f) = { r with localBB = f r.localBB }
                }
            let globalBB =
                { new Lens<OpcViewer.Base.Picking.OpcData, Aardvark.Base.Box3d>() with
                    override x.Get(r) = r.globalBB
                    override x.Set(r,v) = { r with globalBB = v }
                    override x.Update(r,f) = { r with globalBB = f r.globalBB }
                }
    
    
    type MPickingModel(__initial : OpcViewer.Base.Picking.PickingModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<OpcViewer.Base.Picking.PickingModel> = Aardvark.Base.Incremental.EqModRef<OpcViewer.Base.Picking.PickingModel>(__initial) :> Aardvark.Base.Incremental.IModRef<OpcViewer.Base.Picking.PickingModel>
        let _pickingInfos = MMap.Create(__initial.pickingInfos, (fun v -> MOpcData.Create(v)), (fun (m,v) -> MOpcData.Update(m, v)), (fun v -> v))
        let _hitPointsInfo = MMap.Create(__initial.hitPointsInfo)
        let _intersectionPoints = MList.Create(__initial.intersectionPoints)
        
        member x.pickingInfos = _pickingInfos :> amap<_,_>
        member x.hitPointsInfo = _hitPointsInfo :> amap<_,_>
        member x.intersectionPoints = _intersectionPoints :> alist<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : OpcViewer.Base.Picking.PickingModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MMap.Update(_pickingInfos, v.pickingInfos)
                MMap.Update(_hitPointsInfo, v.hitPointsInfo)
                MList.Update(_intersectionPoints, v.intersectionPoints)
                
        
        static member Create(__initial : OpcViewer.Base.Picking.PickingModel) : MPickingModel = MPickingModel(__initial)
        static member Update(m : MPickingModel, v : OpcViewer.Base.Picking.PickingModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<OpcViewer.Base.Picking.PickingModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module PickingModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let pickingInfos =
                { new Lens<OpcViewer.Base.Picking.PickingModel, Aardvark.Base.hmap<Aardvark.Base.Box3d,OpcViewer.Base.Picking.OpcData>>() with
                    override x.Get(r) = r.pickingInfos
                    override x.Set(r,v) = { r with pickingInfos = v }
                    override x.Update(r,f) = { r with pickingInfos = f r.pickingInfos }
                }
            let hitPointsInfo =
                { new Lens<OpcViewer.Base.Picking.PickingModel, Aardvark.Base.hmap<Aardvark.Base.V3d,Aardvark.Base.Box3d>>() with
                    override x.Get(r) = r.hitPointsInfo
                    override x.Set(r,v) = { r with hitPointsInfo = v }
                    override x.Update(r,f) = { r with hitPointsInfo = f r.hitPointsInfo }
                }
            let intersectionPoints =
                { new Lens<OpcViewer.Base.Picking.PickingModel, Aardvark.Base.plist<Aardvark.Base.V3d>>() with
                    override x.Get(r) = r.intersectionPoints
                    override x.Set(r,v) = { r with intersectionPoints = v }
                    override x.Update(r,f) = { r with intersectionPoints = f r.intersectionPoints }
                }

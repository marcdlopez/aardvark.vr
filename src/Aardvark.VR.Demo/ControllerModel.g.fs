namespace Demo

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Demo

[<AutoOpen>]
module Mutable =

    
    
    type MControllerInfo(__initial : Demo.ControllerInfo) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Demo.ControllerInfo> = Aardvark.Base.Incremental.EqModRef<Demo.ControllerInfo>(__initial) :> Aardvark.Base.Incremental.IModRef<Demo.ControllerInfo>
        let _kind = ResetMod.Create(__initial.kind)
        let _buttonKind = ResetMod.Create(__initial.buttonKind)
        let _pose = Aardvark.Vr.Mutable.MPose.Create(__initial.pose)
        let _backButtonPressed = ResetMod.Create(__initial.backButtonPressed)
        let _frontButtonPressed = ResetMod.Create(__initial.frontButtonPressed)
        let _joystickPressed = ResetMod.Create(__initial.joystickPressed)
        
        member x.kind = _kind :> IMod<_>
        member x.buttonKind = _buttonKind :> IMod<_>
        member x.pose = _pose
        member x.backButtonPressed = _backButtonPressed :> IMod<_>
        member x.frontButtonPressed = _frontButtonPressed :> IMod<_>
        member x.joystickPressed = _joystickPressed :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Demo.ControllerInfo) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_kind,v.kind)
                ResetMod.Update(_buttonKind,v.buttonKind)
                Aardvark.Vr.Mutable.MPose.Update(_pose, v.pose)
                ResetMod.Update(_backButtonPressed,v.backButtonPressed)
                ResetMod.Update(_frontButtonPressed,v.frontButtonPressed)
                ResetMod.Update(_joystickPressed,v.joystickPressed)
                
        
        static member Create(__initial : Demo.ControllerInfo) : MControllerInfo = MControllerInfo(__initial)
        static member Update(m : MControllerInfo, v : Demo.ControllerInfo) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Demo.ControllerInfo> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module ControllerInfo =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let kind =
                { new Lens<Demo.ControllerInfo, Demo.ControllerKind>() with
                    override x.Get(r) = r.kind
                    override x.Set(r,v) = { r with kind = v }
                    override x.Update(r,f) = { r with kind = f r.kind }
                }
            let buttonKind =
                { new Lens<Demo.ControllerInfo, Demo.ControllerButtons>() with
                    override x.Get(r) = r.buttonKind
                    override x.Set(r,v) = { r with buttonKind = v }
                    override x.Update(r,f) = { r with buttonKind = f r.buttonKind }
                }
            let pose =
                { new Lens<Demo.ControllerInfo, Aardvark.Vr.Pose>() with
                    override x.Get(r) = r.pose
                    override x.Set(r,v) = { r with pose = v }
                    override x.Update(r,f) = { r with pose = f r.pose }
                }
            let backButtonPressed =
                { new Lens<Demo.ControllerInfo, System.Boolean>() with
                    override x.Get(r) = r.backButtonPressed
                    override x.Set(r,v) = { r with backButtonPressed = v }
                    override x.Update(r,f) = { r with backButtonPressed = f r.backButtonPressed }
                }
            let frontButtonPressed =
                { new Lens<Demo.ControllerInfo, System.Boolean>() with
                    override x.Get(r) = r.frontButtonPressed
                    override x.Set(r,v) = { r with frontButtonPressed = v }
                    override x.Update(r,f) = { r with frontButtonPressed = f r.frontButtonPressed }
                }
            let joystickPressed =
                { new Lens<Demo.ControllerInfo, System.Boolean>() with
                    override x.Get(r) = r.joystickPressed
                    override x.Set(r,v) = { r with joystickPressed = v }
                    override x.Update(r,f) = { r with joystickPressed = f r.joystickPressed }
                }
    
    
    type MControllerModel(__initial : Demo.ControllerModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Demo.ControllerModel> = Aardvark.Base.Incremental.EqModRef<Demo.ControllerModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Demo.ControllerModel>
        let _controllerInfos = MMap.Create(__initial.controllerInfos, (fun v -> MControllerInfo.Create(v)), (fun (m,v) -> MControllerInfo.Update(m, v)), (fun v -> v))
        
        member x.controllerInfos = _controllerInfos :> amap<_,_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Demo.ControllerModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MMap.Update(_controllerInfos, v.controllerInfos)
                
        
        static member Create(__initial : Demo.ControllerModel) : MControllerModel = MControllerModel(__initial)
        static member Update(m : MControllerModel, v : Demo.ControllerModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Demo.ControllerModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module ControllerModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let controllerInfos =
                { new Lens<Demo.ControllerModel, Aardvark.Base.hmap<Demo.ControllerKind,Demo.ControllerInfo>>() with
                    override x.Get(r) = r.controllerInfos
                    override x.Set(r,v) = { r with controllerInfos = v }
                    override x.Update(r,f) = { r with controllerInfos = f r.controllerInfos }
                }

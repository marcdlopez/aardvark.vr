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
        let _sideButtonPressed = ResetMod.Create(__initial.sideButtonPressed)
        let _homeButtonPressed = ResetMod.Create(__initial.homeButtonPressed)
        let _joystickHold = ResetMod.Create(__initial.joystickHold)
        
        member x.kind = _kind :> IMod<_>
        member x.buttonKind = _buttonKind :> IMod<_>
        member x.pose = _pose
        member x.backButtonPressed = _backButtonPressed :> IMod<_>
        member x.frontButtonPressed = _frontButtonPressed :> IMod<_>
        member x.joystickPressed = _joystickPressed :> IMod<_>
        member x.sideButtonPressed = _sideButtonPressed :> IMod<_>
        member x.homeButtonPressed = _homeButtonPressed :> IMod<_>
        member x.joystickHold = _joystickHold :> IMod<_>
        
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
                ResetMod.Update(_sideButtonPressed,v.sideButtonPressed)
                ResetMod.Update(_homeButtonPressed,v.homeButtonPressed)
                ResetMod.Update(_joystickHold,v.joystickHold)
                
        
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
            let sideButtonPressed =
                { new Lens<Demo.ControllerInfo, System.Boolean>() with
                    override x.Get(r) = r.sideButtonPressed
                    override x.Set(r,v) = { r with sideButtonPressed = v }
                    override x.Update(r,f) = { r with sideButtonPressed = f r.sideButtonPressed }
                }
            let homeButtonPressed =
                { new Lens<Demo.ControllerInfo, System.Boolean>() with
                    override x.Get(r) = r.homeButtonPressed
                    override x.Set(r,v) = { r with homeButtonPressed = v }
                    override x.Update(r,f) = { r with homeButtonPressed = f r.homeButtonPressed }
                }
            let joystickHold =
                { new Lens<Demo.ControllerInfo, System.Boolean>() with
                    override x.Get(r) = r.joystickHold
                    override x.Set(r,v) = { r with joystickHold = v }
                    override x.Update(r,f) = { r with joystickHold = f r.joystickHold }
                }
    
    
    type MVisibleBox(__initial : Demo.VisibleBox) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Demo.VisibleBox> = Aardvark.Base.Incremental.EqModRef<Demo.VisibleBox>(__initial) :> Aardvark.Base.Incremental.IModRef<Demo.VisibleBox>
        let _geometry = ResetMod.Create(__initial.geometry)
        let _color = ResetMod.Create(__initial.color)
        let _trafo = ResetMod.Create(__initial.trafo)
        let _flagHovered = ResetMod.Create(__initial.flagHovered)
        let _id = ResetMod.Create(__initial.id)
        
        member x.geometry = _geometry :> IMod<_>
        member x.color = _color :> IMod<_>
        member x.trafo = _trafo :> IMod<_>
        member x.flagHovered = _flagHovered :> IMod<_>
        member x.id = _id :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Demo.VisibleBox) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_geometry,v.geometry)
                ResetMod.Update(_color,v.color)
                ResetMod.Update(_trafo,v.trafo)
                ResetMod.Update(_flagHovered,v.flagHovered)
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
            let flagHovered =
                { new Lens<Demo.VisibleBox, System.Boolean>() with
                    override x.Get(r) = r.flagHovered
                    override x.Set(r,v) = { r with flagHovered = v }
                    override x.Update(r,f) = { r with flagHovered = f r.flagHovered }
                }
            let id =
                { new Lens<Demo.VisibleBox, System.String>() with
                    override x.Get(r) = r.id
                    override x.Set(r,v) = { r with id = v }
                    override x.Update(r,f) = { r with id = f r.id }
                }
    
    
    type MVisibleSphere(__initial : Demo.VisibleSphere) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Demo.VisibleSphere> = Aardvark.Base.Incremental.EqModRef<Demo.VisibleSphere>(__initial) :> Aardvark.Base.Incremental.IModRef<Demo.VisibleSphere>
        let _geometry = ResetMod.Create(__initial.geometry)
        let _color = ResetMod.Create(__initial.color)
        let _trafo = ResetMod.Create(__initial.trafo)
        let _radius = ResetMod.Create(__initial.radius)
        let _distance = ResetMod.Create(__initial.distance)
        let _id = ResetMod.Create(__initial.id)
        
        member x.geometry = _geometry :> IMod<_>
        member x.color = _color :> IMod<_>
        member x.trafo = _trafo :> IMod<_>
        member x.radius = _radius :> IMod<_>
        member x.distance = _distance :> IMod<_>
        member x.id = _id :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Demo.VisibleSphere) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_geometry,v.geometry)
                ResetMod.Update(_color,v.color)
                ResetMod.Update(_trafo,v.trafo)
                ResetMod.Update(_radius,v.radius)
                ResetMod.Update(_distance,v.distance)
                _id.Update(v.id)
                
        
        static member Create(__initial : Demo.VisibleSphere) : MVisibleSphere = MVisibleSphere(__initial)
        static member Update(m : MVisibleSphere, v : Demo.VisibleSphere) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Demo.VisibleSphere> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module VisibleSphere =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let geometry =
                { new Lens<Demo.VisibleSphere, Aardvark.Base.Sphere3d>() with
                    override x.Get(r) = r.geometry
                    override x.Set(r,v) = { r with geometry = v }
                    override x.Update(r,f) = { r with geometry = f r.geometry }
                }
            let color =
                { new Lens<Demo.VisibleSphere, Aardvark.Base.C4b>() with
                    override x.Get(r) = r.color
                    override x.Set(r,v) = { r with color = v }
                    override x.Update(r,f) = { r with color = f r.color }
                }
            let trafo =
                { new Lens<Demo.VisibleSphere, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.trafo
                    override x.Set(r,v) = { r with trafo = v }
                    override x.Update(r,f) = { r with trafo = f r.trafo }
                }
            let radius =
                { new Lens<Demo.VisibleSphere, System.Double>() with
                    override x.Get(r) = r.radius
                    override x.Set(r,v) = { r with radius = v }
                    override x.Update(r,f) = { r with radius = f r.radius }
                }
            let distance =
                { new Lens<Demo.VisibleSphere, System.String>() with
                    override x.Get(r) = r.distance
                    override x.Set(r,v) = { r with distance = v }
                    override x.Update(r,f) = { r with distance = f r.distance }
                }
            let id =
                { new Lens<Demo.VisibleSphere, System.String>() with
                    override x.Get(r) = r.id
                    override x.Set(r,v) = { r with id = v }
                    override x.Update(r,f) = { r with id = f r.id }
                }
    
    
    type MVisibleCylinder(__initial : Demo.VisibleCylinder) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Demo.VisibleCylinder> = Aardvark.Base.Incremental.EqModRef<Demo.VisibleCylinder>(__initial) :> Aardvark.Base.Incremental.IModRef<Demo.VisibleCylinder>
        let _geometry = ResetMod.Create(__initial.geometry)
        let _color = ResetMod.Create(__initial.color)
        let _trafo = ResetMod.Create(__initial.trafo)
        let _radius = ResetMod.Create(__initial.radius)
        let _angle = ResetMod.Create(__initial.angle)
        let _id = ResetMod.Create(__initial.id)
        
        member x.geometry = _geometry :> IMod<_>
        member x.color = _color :> IMod<_>
        member x.trafo = _trafo :> IMod<_>
        member x.radius = _radius :> IMod<_>
        member x.angle = _angle :> IMod<_>
        member x.id = _id :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Demo.VisibleCylinder) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_geometry,v.geometry)
                ResetMod.Update(_color,v.color)
                ResetMod.Update(_trafo,v.trafo)
                ResetMod.Update(_radius,v.radius)
                ResetMod.Update(_angle,v.angle)
                _id.Update(v.id)
                
        
        static member Create(__initial : Demo.VisibleCylinder) : MVisibleCylinder = MVisibleCylinder(__initial)
        static member Update(m : MVisibleCylinder, v : Demo.VisibleCylinder) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Demo.VisibleCylinder> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module VisibleCylinder =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let geometry =
                { new Lens<Demo.VisibleCylinder, Aardvark.Base.Cylinder3d>() with
                    override x.Get(r) = r.geometry
                    override x.Set(r,v) = { r with geometry = v }
                    override x.Update(r,f) = { r with geometry = f r.geometry }
                }
            let color =
                { new Lens<Demo.VisibleCylinder, Aardvark.Base.C4b>() with
                    override x.Get(r) = r.color
                    override x.Set(r,v) = { r with color = v }
                    override x.Update(r,f) = { r with color = f r.color }
                }
            let trafo =
                { new Lens<Demo.VisibleCylinder, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.trafo
                    override x.Set(r,v) = { r with trafo = v }
                    override x.Update(r,f) = { r with trafo = f r.trafo }
                }
            let radius =
                { new Lens<Demo.VisibleCylinder, System.Double>() with
                    override x.Get(r) = r.radius
                    override x.Set(r,v) = { r with radius = v }
                    override x.Update(r,f) = { r with radius = f r.radius }
                }
            let angle =
                { new Lens<Demo.VisibleCylinder, System.String>() with
                    override x.Get(r) = r.angle
                    override x.Set(r,v) = { r with angle = v }
                    override x.Update(r,f) = { r with angle = f r.angle }
                }
            let id =
                { new Lens<Demo.VisibleCylinder, System.String>() with
                    override x.Get(r) = r.id
                    override x.Set(r,v) = { r with id = v }
                    override x.Update(r,f) = { r with id = f r.id }
                }

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
        let _size = ResetMod.Create(__initial.size)
        let _id = ResetMod.Create(__initial.id)
        
        member x.geometry = _geometry :> IMod<_>
        member x.color = _color :> IMod<_>
        member x.trafo = _trafo :> IMod<_>
        member x.size = _size :> IMod<_>
        member x.id = _id :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Demo.VisibleBox) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_geometry,v.geometry)
                ResetMod.Update(_color,v.color)
                ResetMod.Update(_trafo,v.trafo)
                ResetMod.Update(_size,v.size)
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
            let size =
                { new Lens<Demo.VisibleBox, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.size
                    override x.Set(r,v) = { r with size = v }
                    override x.Update(r,f) = { r with size = f r.size }
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
        let _boxes = MList.Create(__initial.boxes, (fun v -> MVisibleBox.Create(v)), (fun (m,v) -> MVisibleBox.Update(m, v)), (fun v -> v))
        let _boxHovered = MOption.Create(__initial.boxHovered)
        let _boxSelected = MSet.Create(__initial.boxSelected)
        let _cameraState = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.cameraState)
        let _ControllerPosition = ResetMod.Create(__initial.ControllerPosition)
        let _offsetToCenter = ResetMod.Create(__initial.offsetToCenter)
        let _isPressed = ResetMod.Create(__initial.isPressed)
        let _boxDistance = ResetMod.Create(__initial.boxDistance)
        let _startingLinePos = ResetMod.Create(__initial.startingLinePos)
        let _endingLinePos = ResetMod.Create(__initial.endingLinePos)
        let _lines = ResetMod.Create(__initial.lines)
        let _grabbed = MSet.Create(__initial.grabbed)
        let _controllerPositions = MMap.Create(__initial.controllerPositions, (fun v -> Aardvark.Vr.Mutable.MPose.Create(v)), (fun (m,v) -> Aardvark.Vr.Mutable.MPose.Update(m, v)), (fun v -> v))
        
        member x.text = _text :> IMod<_>
        member x.vr = _vr :> IMod<_>
        member x.boxes = _boxes :> alist<_>
        member x.boxHovered = _boxHovered :> IMod<_>
        member x.boxSelected = _boxSelected :> aset<_>
        member x.cameraState = _cameraState
        member x.ControllerPosition = _ControllerPosition :> IMod<_>
        member x.offsetToCenter = _offsetToCenter :> IMod<_>
        member x.isPressed = _isPressed :> IMod<_>
        member x.boxDistance = _boxDistance :> IMod<_>
        member x.startingLinePos = _startingLinePos :> IMod<_>
        member x.endingLinePos = _endingLinePos :> IMod<_>
        member x.lines = _lines :> IMod<_>
        member x.grabbed = _grabbed :> aset<_>
        member x.controllerPositions = _controllerPositions :> amap<_,_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Demo.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_text,v.text)
                ResetMod.Update(_vr,v.vr)
                MList.Update(_boxes, v.boxes)
                MOption.Update(_boxHovered, v.boxHovered)
                MSet.Update(_boxSelected, v.boxSelected)
                Aardvark.UI.Primitives.Mutable.MCameraControllerState.Update(_cameraState, v.cameraState)
                ResetMod.Update(_ControllerPosition,v.ControllerPosition)
                ResetMod.Update(_offsetToCenter,v.offsetToCenter)
                ResetMod.Update(_isPressed,v.isPressed)
                ResetMod.Update(_boxDistance,v.boxDistance)
                ResetMod.Update(_startingLinePos,v.startingLinePos)
                ResetMod.Update(_endingLinePos,v.endingLinePos)
                ResetMod.Update(_lines,v.lines)
                MSet.Update(_grabbed, v.grabbed)
                MMap.Update(_controllerPositions, v.controllerPositions)
                
        
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
            let boxes =
                { new Lens<Demo.Model, Aardvark.Base.plist<Demo.VisibleBox>>() with
                    override x.Get(r) = r.boxes
                    override x.Set(r,v) = { r with boxes = v }
                    override x.Update(r,f) = { r with boxes = f r.boxes }
                }
            let boxHovered =
                { new Lens<Demo.Model, Microsoft.FSharp.Core.Option<System.String>>() with
                    override x.Get(r) = r.boxHovered
                    override x.Set(r,v) = { r with boxHovered = v }
                    override x.Update(r,f) = { r with boxHovered = f r.boxHovered }
                }
            let boxSelected =
                { new Lens<Demo.Model, Aardvark.Base.hset<System.String>>() with
                    override x.Get(r) = r.boxSelected
                    override x.Set(r,v) = { r with boxSelected = v }
                    override x.Update(r,f) = { r with boxSelected = f r.boxSelected }
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
            let isPressed =
                { new Lens<Demo.Model, System.Boolean>() with
                    override x.Get(r) = r.isPressed
                    override x.Set(r,v) = { r with isPressed = v }
                    override x.Update(r,f) = { r with isPressed = f r.isPressed }
                }
            let boxDistance =
                { new Lens<Demo.Model, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.boxDistance
                    override x.Set(r,v) = { r with boxDistance = v }
                    override x.Update(r,f) = { r with boxDistance = f r.boxDistance }
                }
            let startingLinePos =
                { new Lens<Demo.Model, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.startingLinePos
                    override x.Set(r,v) = { r with startingLinePos = v }
                    override x.Update(r,f) = { r with startingLinePos = f r.startingLinePos }
                }
            let endingLinePos =
                { new Lens<Demo.Model, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.endingLinePos
                    override x.Set(r,v) = { r with endingLinePos = v }
                    override x.Update(r,f) = { r with endingLinePos = f r.endingLinePos }
                }
            let lines =
                { new Lens<Demo.Model, Aardvark.Base.Line3d[]>() with
                    override x.Get(r) = r.lines
                    override x.Set(r,v) = { r with lines = v }
                    override x.Update(r,f) = { r with lines = f r.lines }
                }
            let grabbed =
                { new Lens<Demo.Model, Aardvark.Base.hset<System.String>>() with
                    override x.Get(r) = r.grabbed
                    override x.Set(r,v) = { r with grabbed = v }
                    override x.Update(r,f) = { r with grabbed = f r.grabbed }
                }
            let controllerPositions =
                { new Lens<Demo.Model, Aardvark.Base.hmap<System.Int32,Aardvark.Vr.Pose>>() with
                    override x.Get(r) = r.controllerPositions
                    override x.Set(r,v) = { r with controllerPositions = v }
                    override x.Update(r,f) = { r with controllerPositions = f r.controllerPositions }
                }

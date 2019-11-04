namespace Demo

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Demo

[<AutoOpen>]
module Mutable =

    
    
    type MMenuModel(__initial : Demo.MenuModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Demo.MenuModel> = Aardvark.Base.Incremental.EqModRef<Demo.MenuModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Demo.MenuModel>
        let _mainMenuBoxes = MList.Create(__initial.mainMenuBoxes, (fun v -> MVisibleBox.Create(v)), (fun (m,v) -> MVisibleBox.Update(m, v)), (fun v -> v))
        let _boxHovered = MOption.Create(__initial.boxHovered)
        let _subMenuBoxes = MList.Create(__initial.subMenuBoxes, (fun v -> MVisibleBox.Create(v)), (fun (m,v) -> MVisibleBox.Update(m, v)), (fun v -> v))
        let _menu = ResetMod.Create(__initial.menu)
        let _subMenu = ResetMod.Create(__initial.subMenu)
        let _initialMenuState = ResetMod.Create(__initial.initialMenuState)
        let _menuButtonPressed = ResetMod.Create(__initial.menuButtonPressed)
        let _initialMenuPosition = Aardvark.Vr.Mutable.MPose.Create(__initial.initialMenuPosition)
        let _initialMenuPositionBool = ResetMod.Create(__initial.initialMenuPositionBool)
        let _controllerMenuSelector = ResetMod.Create(__initial.controllerMenuSelector)
        
        member x.mainMenuBoxes = _mainMenuBoxes :> alist<_>
        member x.boxHovered = _boxHovered :> IMod<_>
        member x.subMenuBoxes = _subMenuBoxes :> alist<_>
        member x.menu = _menu :> IMod<_>
        member x.subMenu = _subMenu :> IMod<_>
        member x.initialMenuState = _initialMenuState :> IMod<_>
        member x.menuButtonPressed = _menuButtonPressed :> IMod<_>
        member x.initialMenuPosition = _initialMenuPosition
        member x.initialMenuPositionBool = _initialMenuPositionBool :> IMod<_>
        member x.controllerMenuSelector = _controllerMenuSelector :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Demo.MenuModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MList.Update(_mainMenuBoxes, v.mainMenuBoxes)
                MOption.Update(_boxHovered, v.boxHovered)
                MList.Update(_subMenuBoxes, v.subMenuBoxes)
                ResetMod.Update(_menu,v.menu)
                ResetMod.Update(_subMenu,v.subMenu)
                ResetMod.Update(_initialMenuState,v.initialMenuState)
                ResetMod.Update(_menuButtonPressed,v.menuButtonPressed)
                Aardvark.Vr.Mutable.MPose.Update(_initialMenuPosition, v.initialMenuPosition)
                ResetMod.Update(_initialMenuPositionBool,v.initialMenuPositionBool)
                ResetMod.Update(_controllerMenuSelector,v.controllerMenuSelector)
                
        
        static member Create(__initial : Demo.MenuModel) : MMenuModel = MMenuModel(__initial)
        static member Update(m : MMenuModel, v : Demo.MenuModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Demo.MenuModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module MenuModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let mainMenuBoxes =
                { new Lens<Demo.MenuModel, Aardvark.Base.plist<Demo.VisibleBox>>() with
                    override x.Get(r) = r.mainMenuBoxes
                    override x.Set(r,v) = { r with mainMenuBoxes = v }
                    override x.Update(r,f) = { r with mainMenuBoxes = f r.mainMenuBoxes }
                }
            let boxHovered =
                { new Lens<Demo.MenuModel, Microsoft.FSharp.Core.Option<System.String>>() with
                    override x.Get(r) = r.boxHovered
                    override x.Set(r,v) = { r with boxHovered = v }
                    override x.Update(r,f) = { r with boxHovered = f r.boxHovered }
                }
            let subMenuBoxes =
                { new Lens<Demo.MenuModel, Aardvark.Base.plist<Demo.VisibleBox>>() with
                    override x.Get(r) = r.subMenuBoxes
                    override x.Set(r,v) = { r with subMenuBoxes = v }
                    override x.Update(r,f) = { r with subMenuBoxes = f r.subMenuBoxes }
                }
            let menu =
                { new Lens<Demo.MenuModel, Demo.MenuState>() with
                    override x.Get(r) = r.menu
                    override x.Set(r,v) = { r with menu = v }
                    override x.Update(r,f) = { r with menu = f r.menu }
                }
            let subMenu =
                { new Lens<Demo.MenuModel, Demo.subMenuState>() with
                    override x.Get(r) = r.subMenu
                    override x.Set(r,v) = { r with subMenu = v }
                    override x.Update(r,f) = { r with subMenu = f r.subMenu }
                }
            let initialMenuState =
                { new Lens<Demo.MenuModel, Demo.MenuState>() with
                    override x.Get(r) = r.initialMenuState
                    override x.Set(r,v) = { r with initialMenuState = v }
                    override x.Update(r,f) = { r with initialMenuState = f r.initialMenuState }
                }
            let menuButtonPressed =
                { new Lens<Demo.MenuModel, System.Boolean>() with
                    override x.Get(r) = r.menuButtonPressed
                    override x.Set(r,v) = { r with menuButtonPressed = v }
                    override x.Update(r,f) = { r with menuButtonPressed = f r.menuButtonPressed }
                }
            let initialMenuPosition =
                { new Lens<Demo.MenuModel, Aardvark.Vr.Pose>() with
                    override x.Get(r) = r.initialMenuPosition
                    override x.Set(r,v) = { r with initialMenuPosition = v }
                    override x.Update(r,f) = { r with initialMenuPosition = f r.initialMenuPosition }
                }
            let initialMenuPositionBool =
                { new Lens<Demo.MenuModel, System.Boolean>() with
                    override x.Get(r) = r.initialMenuPositionBool
                    override x.Set(r,v) = { r with initialMenuPositionBool = v }
                    override x.Update(r,f) = { r with initialMenuPositionBool = f r.initialMenuPositionBool }
                }
            let controllerMenuSelector =
                { new Lens<Demo.MenuModel, Demo.ControllerKind>() with
                    override x.Get(r) = r.controllerMenuSelector
                    override x.Set(r,v) = { r with controllerMenuSelector = v }
                    override x.Update(r,f) = { r with controllerMenuSelector = f r.controllerMenuSelector }
                }

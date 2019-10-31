namespace Demo

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.Rendering.Text
open Aardvark.SceneGraph
open Aardvark.UI
open Aardvark.UI.Trafos
open Aardvark.Vr

open OpcViewer.Base

module MenuApp = 
    
    type MenuMessage = 
    | CreateMenu of ControllerKind * bool
    | UpdateControllerPose of Pose
    | Select
    | CloseMenu

    let update (state : VrState) (vr : VrActions) (model : MenuModel) (msg : MenuMessage) : MenuModel = 
        match msg with
        | CreateMenu (kind, buttonPressed) -> 
            model
        | UpdateControllerPose p -> model
        | Select -> model 
        | CloseMenu -> model
            
    let input (msg : VrMessage) =
        match msg with
        // buttons identifications: sensitive = 0, backButton = 1, sideButtons = 2
        | VrMessage.Touch(con,button) -> 
            match button with 
            | 0 -> [CreateMenu(con |> ControllerKind.fromInt, true)]
            | _ -> []
        | VrMessage.Untouch(con,button) -> 
            match button with 
            | 0 -> [CreateMenu(con |> ControllerKind.fromInt, false)]
            | _ -> []
        | VrMessage.UpdatePose(cn,p) -> 
            if p.isValid then 
                [UpdateControllerPose(p)]
            else []
        | VrMessage.Press(con,button) -> 
            match button with
            | 0 -> [Select]
            | _ -> []//Select?
        | VrMessage.Unpress(con,button) -> 
            match button with 
            | 0 -> [Select]
            | _ -> []//UnSelect?
        | _ -> 
            []

    
    let ui (info : VrSystemInfo) (m : MMenuModel) : DomNode<MenuMessage> = failwith""

    let vr (info : VrSystemInfo) (m : MMenuModel) : ISg<'a> = failwith ""
    
    let threads (model : MenuModel) = 
        ThreadPool.empty

    let pause (info : VrSystemInfo) (m : MMenuModel) =
        Sg.box' C4b.Red Box3d.Unit
        |> Sg.noEvents
        |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! DefaultSurfaces.vertexColor
            do! DefaultSurfaces.simpleLighting
        }

    let initial =
        {
            menu                    = MenuState.Navigation
            controllerMenuSelector  = ControllerKind.ControllerA
            subMenu                 = subMenuState.Draw
            initialMenuState        = MenuState.Annotation
            menuButtonPressed       = false
            initialMenuPosition     = Pose.none
            initialMenuPositionBool = false
        }
    let app =
        {
            unpersist = Unpersist.instance
            initial = initial
            update = update
            threads = threads
            input = input 
            ui = ui
            vr = vr
            pauseScene = Some pause
        }
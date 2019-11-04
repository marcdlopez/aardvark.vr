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

    let rec update (state : VrState) (vr : VrActions) (model : MenuModel) (msg : MenuMessage) : MenuModel = 
        match msg with
        | CreateMenu (kind, buttonPressed) -> 
            let model = 
                if not(model.initialMenuPositionBool) then 
                    let controllerPos = model.controllerInfos |> HMap.tryFind kind
                    match controllerPos with
                    | Some id -> 
                        {model with initialMenuPosition = id.pose; initialMenuPositionBool = true}
                    | None -> model
                else model

            if buttonPressed then 
                let hmdPos = model.controllerInfos |> HMap.values |> Seq.item 0
                match model.menu with
                | Navigation ->
                    let newMenuBoxes = OpcUtilities.mkBoxesMenu model.initialMenuPosition hmdPos.pose 3 //number of menu possibilities should be the number of boxes. So far 2
                    let box0id = newMenuBoxes |> Seq.item 0
                    let box1id = newMenuBoxes |> Seq.item 1
                    let newMenuBoxes = 
                        newMenuBoxes 
                        |> PList.map (fun idx -> 
                            if idx.id.Equals(box0id.id) then {idx with id = "Reset"}
                            else if idx.id.Equals(box1id.id) then {idx with id = "Navigation"}
                            else {idx with id = "Annotation"}
                            )
                    {model with mainMenuBoxes = newMenuBoxes; menuButtonPressed = buttonPressed}
            else model
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
            controllerInfos         = HMap.empty
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
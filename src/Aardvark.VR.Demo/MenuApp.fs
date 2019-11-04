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

    let update (cm : UtilitiesModel) (state : VrState) (vr : VrActions) (model : MenuModel) (msg : MenuMessage)  : MenuModel = 
        match msg with
        | CreateMenu (kind, buttonPressed) -> 
            let model = 
                if not(model.initialMenuPositionBool) then 
                    let controllerPos = cm.controllerInfos |> HMap.tryFind kind
                    match controllerPos with
                    | Some id -> 
                        {model with initialMenuPosition = id.pose; initialMenuPositionBool = true}
                    | None -> model
                else model

            if buttonPressed then 
                let hmdPos = cm.controllerInfos |> HMap.values |> Seq.item 0
                match model.menu with
                | Navigation ->
                    let newMenuBoxes = UtilitiesMenu.mkBoxesMenu model.initialMenuPosition hmdPos.pose 3 //number of menu possibilities should be the number of boxes. So far 2
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
                | Annotation -> 
                    let newSubMenuBoxes = UtilitiesMenu.mkBoxesMenu model.initialMenuPosition hmdPos.pose 6
                    let boxID0 = newSubMenuBoxes |> Seq.item 0
                    let boxID1 = newSubMenuBoxes |> Seq.item 1 
                    let boxID2 = newSubMenuBoxes |> Seq.item 2
                    let boxID3 = newSubMenuBoxes |> Seq.item 3
                    let boxID4 = newSubMenuBoxes |> Seq.item 4
                    let newSubMenuBoxes = 
                        newSubMenuBoxes
                        |> PList.map (fun idx -> 
                            if idx.id.Equals(boxID0.id)then {idx with id = "Back"}
                            else if idx.id.Equals(boxID1.id) then {idx with id = "Reset"}
                            else if idx.id.Equals(boxID2.id) then {idx with id = "Flag"}
                            else if idx.id.Equals(boxID3.id) then {idx with id = "Dip and Strike"}
                            else if idx.id.Equals(boxID4.id) then {idx with id = "Draw"} //allow different options in the draw mode: freely draw and draw by points
                            else {idx with id = "Line"})
                    {model with subMenuBoxes = newSubMenuBoxes; menuButtonPressed = buttonPressed}
                | MainReset -> 
                    {model with menu = MenuState.Navigation}
            else 
                {model with mainMenuBoxes = PList.empty; subMenuBoxes = PList.empty; menuButtonPressed = buttonPressed; initialMenuPositionBool = false}
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

    
    let ui (info : VrSystemInfo) (m : MMenuModel) : DomNode<MenuMessage> = DomNode.Empty()

    let mkColor (model : MMenuModel) (box : MVisibleBox) =
        let id = box.id

        let color = 
            id
            |> Mod.bind (fun s ->

                let hoverColor =
                    model.boxHovered 
                    |> Mod.bind (function 
                        | Some k -> if k = s then Mod.constant C4b.Blue else box.color
                        | None -> box.color
                    )
                hoverColor
            )
        color
    
    let mkISg (model : MMenuModel) (box : MVisibleBox) =
        let color = mkColor model box
        let pos = box.trafo
        let font = Font.create "Consolas" FontStyle.Regular

        let menuText = 
            box.geometry |> Mod.map ( fun box1 -> 
                Sg.text font C4b.White box.id
                    |> Sg.noEvents
                    |> Sg.trafo(Mod.constant(Trafo3d.RotationInDegrees(V3d(90.0,0.0,90.0))))
                    |> Sg.scale 0.05
                    |> Sg.trafo(pos)
                    |> Sg.pickable (PickShape.Box (box1))
            )
                |> Sg.dynamic 
        
        let menuBox = 
            Sg.box color box.geometry
                |> Sg.noEvents
                |> Sg.trafo(pos)
                |> Sg.shader {
                    do! DefaultSurfaces.trafo
                    do! DefaultSurfaces.vertexColor
                    //do! DefaultSurfaces.simpleLighting
                    }                    
                |> Sg.fillMode (Mod.constant FillMode.Line)

        menuText
        |> Sg.andAlso menuBox

    let vr (info : VrSystemInfo) (m : MMenuModel) : ISg<'a> = 
        let menuBox = 
            m.mainMenuBoxes
            |> AList.toASet 
            |> ASet.map (fun b -> 
                mkISg m b 
               )
            |> Sg.set
            |> Sg.effect [
                toEffect DefaultSurfaces.trafo
                toEffect DefaultSurfaces.vertexColor
                toEffect DefaultSurfaces.simpleLighting                              
                ]
            |> Sg.noEvents

        let annotationSubMenuBox = 
            m.subMenuBoxes
            |> AList.toASet 
            |> ASet.map (fun b -> 
                mkISg m b 
                )
            |> Sg.set
            |> Sg.effect [
                toEffect DefaultSurfaces.trafo
                toEffect DefaultSurfaces.vertexColor
                toEffect DefaultSurfaces.simpleLighting                              
                ]
            |> Sg.noEvents

        menuBox
        |> Sg.andAlso annotationSubMenuBox

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
            mainMenuBoxes           = PList.empty
            subMenuBoxes            = PList.empty
            boxHovered              = None
        }
    let app =
        {
            unpersist = Unpersist.instance
            initial = initial
            update = update (UtilitiesModel.initial) 
            threads = threads
            input = input 
            ui = ui
            vr = vr
            pauseScene = Some pause
        }
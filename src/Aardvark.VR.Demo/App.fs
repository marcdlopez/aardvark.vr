namespace Demo

open System
open System.IO
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.Rendering.Text
open Aardvark.SceneGraph
open Aardvark.SceneGraph.Opc
open Aardvark.UI
open Aardvark.UI.Primitives
open Aardvark.UI.Trafos
open Aardvark.UI.Generic
open FShade
open Aardvark.Application.OpenVR
open Aardvark.Vr

open OpcViewer.Base
open OpcViewer.Base.Picking
open OpcViewer.Base.Attributes

type MenuMessages  =
    | CreateMenu of  ControllerKind*bool
    | UpdateControllerPosition of Pose
    | Select of Index
    | CloseMenu

type DemoMessage =
| SetText of string 
| ToggleVR
| CreateMenu of ControllerKind*bool
| Select of string
| HoverIn of string
| HoverOut
| CameraMessage         of FreeFlyController.Message    
| SetControllerPosition of ControllerKind *  Pose
| GrabObject            of ControllerKind * ControllerButtons * bool
| TranslateObject       of V3d
| AddBox
| OpcViewerMsg of PickingAction

module Demo =
    open Aardvark.Application
    open Aardvark.VRVis.Opc
    open Aardvark.UI.Primitives
    open Aardvark.Base.Rendering
    open Model
    open OpenTK
    open Valve.VR
    open OpenTK.Input
    
    let show  (att : list<string * AttributeValue<_>>) (sg : ISg<_>) =

        let view (m : MCameraControllerState) =
            let frustum = Frustum.perspective 60.0 0.1 1000.0 1.0 |> Mod.constant
            FreeFlyController.controlledControl m id frustum (AttributeMap.ofList att) sg

        let app =
            {
                initial = FreeFlyController.initial
                update = FreeFlyController.update
                view = view
                threads = FreeFlyController.threads
                unpersist = Unpersist.instance
            }

        subApp' (fun _ _ -> Seq.empty) (fun _ _ -> Seq.empty) [] app


    let rec update (state : VrState) (vr : VrActions) (model : Model) (msg : DemoMessage) : Model =
        match msg with
        | OpcViewerMsg m -> 
            let newOpcModel = OpcViewer.Base.Picking.PickingApp.update OpcViewer.Base.Picking.PickingModel.initial m
            {model with pickingModel = newOpcModel}
        | SetText t -> 
            { model with text = t }
        | ToggleVR ->
            if model.vr then vr.stop()
            else vr.start()
            { model with vr = not model.vr }
        | CreateMenu (kind, buttonPressed) ->
            model
            //let model = 
            //    if not(model.initialMenuPositionBool) then 
            //        let controllerPos = model.controllerInfos |> HMap.tryFind kind
            //        match controllerPos with
            //        | Some id -> 
            //            {model with initialMenuPosition = id.pose; initialMenuPositionBool = true}
            //        | None -> model
            //    else model
            //if buttonPressed then 
            //    let hmdPos = model.controllerInfos |> HMap.values |> Seq.item 0
            //    match model.menu with
            //    | Navigation ->
            //        let newMenuBoxes = OpcUtilities.mkBoxesMenu model.initialMenuPosition hmdPos.pose 3 //number of menu possibilities should be the number of boxes. So far 2
            //        let box0id = newMenuBoxes |> Seq.item 0
            //        let box1id = newMenuBoxes |> Seq.item 1
            //        let newMenuBoxes = 
            //            newMenuBoxes 
            //            |> PList.map (fun idx -> 
            //                if idx.id.Equals(box0id.id) then {idx with id = "Reset"}
            //                else if idx.id.Equals(box1id.id) then {idx with id = "Navigation"}
            //                else {idx with id = "Annotation"}
            //                )
            //        {model with mainMenuBoxes = newMenuBoxes; menuButtonPressed = buttonPressed}
            //    | Annotation -> 
            //        let newSubMenuBoxes = OpcUtilities.mkBoxesMenu model.initialMenuPosition hmdPos.pose 6
            //        let boxID0 = newSubMenuBoxes |> Seq.item 0
            //        let boxID1 = newSubMenuBoxes |> Seq.item 1 
            //        let boxID2 = newSubMenuBoxes |> Seq.item 2
            //        let boxID3 = newSubMenuBoxes |> Seq.item 3
            //        let boxID4 = newSubMenuBoxes |> Seq.item 4
            //        let newSubMenuBoxes = 
            //            newSubMenuBoxes
            //            |> PList.map (fun idx -> 
            //                if idx.id.Equals(boxID0.id)then {idx with id = "Back"}
            //                else if idx.id.Equals(boxID1.id) then {idx with id = "Reset"}
            //                else if idx.id.Equals(boxID2.id) then {idx with id = "Flag"}
            //                else if idx.id.Equals(boxID3.id) then {idx with id = "Dip and Strike"}
            //                else if idx.id.Equals(boxID4.id) then {idx with id = "Draw"} //allow different options in the draw mode: freely draw and draw by points
            //                else {idx with id = "Line"})
            //        {model with subMenuBoxes = newSubMenuBoxes; menuButtonPressed = buttonPressed}
            //    | MainReset -> 
            //        model |> OpcUtilities.resetEverything
            //else 
            //    {model with mainMenuBoxes = PList.empty; subMenuBoxes = PList.empty; menuButtonPressed = buttonPressed; initialMenuPositionBool = false}
            
        | HoverIn id ->
            match model.boxHovered with 
            | Some oldID when id = oldID -> model
            | _ ->
                { model with boxHovered = Some id} 
        | HoverOut ->
            if model.boxHovered.IsSome then
                { model with boxHovered = None}
            else 
                model
        | CameraMessage m -> 
            { model with cameraState = FreeFlyController.update model.cameraState m }   
        | SetControllerPosition (kind, p) ->                         
            let newModel = 
                match model.menu with
                | MenuState.Navigation ->
                    model 
                    |> NavigationOpc.currentSceneInfo kind p
                | MenuState.Annotation ->
                    model
                    |> AnnotationOpc.annotationMode kind p model.annotationMenu
                | MenuState.MainReset -> 
                    model
             
            //let controllerA = newModel.controllerInfos |> HMap.tryFind 
            let newModel =
                 let controllerA = model.controllerInfos |> HMap.tryFind ControllerKind.ControllerA
                 let controllerB = model.controllerInfos |> HMap.tryFind ControllerKind.ControllerB
                 
                 match controllerA, controllerB with
                 | Some a, Some b -> 
                    let mayHoverMenu = OpcUtilities.mayHover newModel.mainMenuBoxes a b
                    match mayHoverMenu with
                     | Some id  -> //SELECT
                        if (a.joystickPressed || b.joystickPressed) then
                            let box0ID = newModel.mainMenuBoxes |> Seq.item 0
                            let box1ID = newModel.mainMenuBoxes |> Seq.item 1

                            let menuSelector = if a.joystickPressed then a.kind else b.kind
                                
                            if box0ID.id = id then 
                                {   newModel with menu = MenuState.MainReset }
                            else if box1ID.id = id then 
                                {   newModel with menu = MenuState.Navigation }
                            else 
                                {
                                    newModel with 
                                        menu = MenuState.Annotation; 
                                        controllerMenuSelector = menuSelector; 
                                        mainMenuBoxes = PList.empty
                                }
                            
                        else //HOVER
                            update state vr newModel (HoverIn id)
                     | _ -> //HOVEROUT
                         update state vr newModel HoverOut
                 | _ -> //DEFAULT
                    newModel

            let newModel = 
                if newModel.menu.Equals(MenuState.Annotation) && newModel.controllerInfos.Count.Equals(5) then 
                    let controller1, controller2 = 
                        newModel 
                        |> OpcUtilities.getControllersInfo 3 4 //these two ints correspond to the id of the controllers
                    let mayHoverSubMenu = OpcUtilities.mayHover newModel.subMenuBoxes controller1 controller2
                    match mayHoverSubMenu with
                    | Some ID -> 
                        if controller2.joystickPressed || controller1.joystickPressed then 
                            let boxID0 = newModel.subMenuBoxes |> Seq.item 0
                            let boxID1 = newModel.subMenuBoxes |> Seq.item 1 
                            let boxID2 = newModel.subMenuBoxes |> Seq.item 2
                            let boxID3 = newModel.subMenuBoxes |> Seq.item 3
                            let boxID4 = newModel.subMenuBoxes |> Seq.item 4
                            
                            if boxID0.id.Contains(ID) then {newModel with menu = MenuState.Navigation}
                            else if boxID1.id.Contains(ID) then {newModel with annotationMenu = subMenuState.Reset}
                            else if boxID2.id.Contains(ID) then{newModel with annotationMenu = subMenuState.Flag}
                            else if boxID3.id.Contains(ID) then{newModel with annotationMenu = subMenuState.DipAndStrike}
                            else if boxID4.id.Contains(ID) then{newModel with annotationMenu = subMenuState.Draw}
                            else {newModel with annotationMenu = subMenuState.Line}
                        else update state vr newModel (HoverIn ID)
                    | None -> update state vr newModel HoverOut
                else {newModel with subMenuBoxes = PList.empty}
            newModel
            
        | GrabObject (kind, buttonKind, buttonPress)-> //TODO ML make enumtype for buttons similar to controllerkind
            
            printfn "Menu mode is: %s when buttonpress is: %s" (model.menu.ToString()) (buttonPress.ToString())
            
            let updateControllerButtons = 
                model.controllerInfos
                |> HMap.alter kind (fun but ->  
                match but with
                | Some x -> 
                    match buttonKind |> ControllerButtons.toInt with 
                    | 0 -> Some {x with joystickPressed = buttonPress}
                    | 1 -> Some {x with backButtonPressed = buttonPress}
                    | _ -> None
                    
                | None -> 
                    match buttonKind |> ControllerButtons.toInt with 
                    | 1 -> 
                        let newInfo = {
                            kind = kind
                            pose = Pose.none
                            buttonKind = buttonKind
                            //buttons  = ButtonStates
                            frontButtonPressed = false
                            backButtonPressed = buttonPress
                            joystickPressed = false
                         }
                        Some newInfo
                    | 0 -> 
                        let newInfo = {
                            kind = kind
                            pose = Pose.none
                            buttonKind = buttonKind
                            //buttons  = ButtonStates
                            frontButtonPressed = false
                            backButtonPressed = false
                            joystickPressed = buttonPress
                         }
                        Some newInfo)

            let newModel = {model with controllerInfos = updateControllerButtons; initialMenuState = model.menu; controllerMenuSelector = kind}
            
            let newModel =  
                match newModel.menuButtonPressed with 
                | true -> 
                    if not(buttonPress) then 
                        update state vr newModel (CreateMenu (newModel.controllerMenuSelector, true))
                    else newModel
                | false -> newModel

            match newModel.menu with 
            | Navigation -> 
                newModel
                |> NavigationOpc.initialSceneInfo
            | Annotation ->
                let controllerPos = newModel.controllerInfos |> HMap.tryFind kind
                match controllerPos with 
                | Some id -> 
                    match newModel.annotationMenu with
                    | Draw -> 
                        match buttonKind |> ControllerButtons.toInt with 
                        | 1 -> 
                            match buttonPress with 
                            | true -> 
                                let preDrawBox = OpcUtilities.mkPointDraw id.pose
                                let newFirstDrawingPoint = 
                                    newModel.drawingPoint
                                    |> PList.prepend preDrawBox
                                    
                                {newModel with drawingPoint = newFirstDrawingPoint}
                            | false -> 
                                newModel
                        | _ -> newModel
                    | _ -> newModel
                | None -> newModel
            | MainReset -> newModel
                
        | _ -> model

    let mkColor (model : MModel) (box : MVisibleBox) =
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
    
    let mkISg (model : MModel) (box : MVisibleBox) =
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
                |> Sg.withEvents [
                    Sg.onEnter (fun _  -> HoverIn  (box.id.ToString()))
                    Sg.onLeave (fun _ -> HoverOut)
                ]     
                |> Sg.fillMode (Mod.constant FillMode.Line)

        menuText
        |> Sg.andAlso menuBox

    let mkDrawingBox (model : MModel) (box : MVisibleBox) =
        let color = box.color
        let pos = box.trafo
        Sg.box color box.geometry
            |> Sg.noEvents
            |> Sg.trafo(pos)
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.vertexColor
                //do! DefaultSurfaces.simpleLighting
                }                

    let threads (model : Model) =
        ThreadPool.empty
        
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
        | VrMessage.PressButton(_,2) ->
            //printfn "Button identification %d" button
            [ToggleVR]
        | VrMessage.UpdatePose(cn,p) -> 
            if p.isValid then 
                let pos = p.deviceToWorld.Forward.TransformPos(V3d.Zero)
                //printfn "%d changed pos= %A" cn pos
                [SetControllerPosition (cn |> ControllerKind.fromInt, p)]
            else []
        | VrMessage.Press(con,button) -> 
            printfn "%d Button identification %d" con button
            match button with
            //| 0 -> [CreateMenu(con, true)]
            | _ -> [GrabObject(con |> ControllerKind.fromInt, button |> ControllerButtons.fromInt, true)]
        | VrMessage.Unpress(con,button) -> 
            printfn "Button unpressed by %d" con
            match button with 
            //| 0 -> [CreateMenu(con, false)]
            | _ -> [GrabObject (con |> ControllerKind.fromInt, button |> ControllerButtons.fromInt, false)]
        | _ -> 
            []


    let mkControllerBox (cp : MPose) =
        //Sg.box' C4b.Cyan Box3d.Unit
        Sg.cone' 20 C4b.Cyan 2.0 3.0 
            |> Sg.noEvents
            |> Sg.scale 0.01
            |> Sg.trafo cp.deviceToWorld

    let ui (info : VrSystemInfo) (m : MModel) =
        let text = m.vr |> Mod.map (function true -> "Stop VR" | false -> "Start VR")
        let textAddBox = Mod.constant "Add Box"
        let distanceToBox = Mod.constant "Distance between boxes" 
        
        let hmd =
            m.vr |> Mod.bind (fun vr ->
                if vr then
                    Mod.map2 (Array.map2 (fun (v : Trafo3d) (p : Trafo3d) -> (v * p).Inverse)) info.render.viewTrafos info.render.projTrafos
                else
                    Mod.constant [|Trafo3d.Translation(100000.0,10000.0,1000.0)|]
            )

        let hmdSg =
            List.init 2 (fun i ->
                Sg.wireBox (Mod.constant C4b.Yellow) (Mod.constant (Box3d(V3d(-1,-1,-1000), V3d(1.0,1.0,-0.9))))
                |> Sg.noEvents
                |> Sg.trafo (hmd |> Mod.map (fun t -> if i < t.Length then t.[i] else Trafo3d.Translation(100000.0,10000.0,1000.0)))
            )
            |> Sg.ofList
            

        let chap =
            match info.bounds with
            | Some bounds ->
                let arr = bounds.EdgeLines |> Seq.toArray
                Sg.lines (Mod.constant C4b.Red) (Mod.constant arr)
                |> Sg.noEvents
                |> Sg.transform (Trafo3d.FromBasis(V3d.IOO, V3d.OOI, -V3d.OIO, V3d.Zero))
            | _ ->
                Sg.empty


        let stuff =
            Sg.ofList [hmdSg; chap]
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.vertexColor
            }


        let frustum =
            Mod.constant (Frustum.perspective 60.0 0.1 100.0 1.0)
  
        let opcs = 
            m.opcInfos
              |> AMap.toASet
              |> ASet.map(fun info -> Sg.createSingleOpcSg m.opcAttributes.selectedScalar (Mod.constant false) m.cameraState.view info)
              |> Sg.set
              |> Sg.effect [ 
                toEffect Shader.stableTrafo
                toEffect DefaultSurfaces.diffuseTexture  
                toEffect Shader.AttributeShader.falseColorLegend //falseColorLegendGray
                ]
              //|> Sg.noEvents  
              

        div [ style "width: 100%; height: 100%" ] [
            FreeFlyController.controlledControl m.cameraState CameraMessage frustum
                (AttributeMap.ofList [
                    attribute "style" "width:65%; height: 100%; float: left;"
                    attribute "data-samples" "8"
                ])
                (
                    //opcs
                    //|> Sg.map (OpcSelectionViewer.Message.PickingAction)
                    //|> Sg.map OpcViewerMsg
                    //|> Sg.noEvents
                    m.mainMenuBoxes 
                    |> AList.toASet 
                    |> ASet.map (function b -> mkISg m b)
                    |> Sg.set
                    |> Sg.effect [
                        toEffect DefaultSurfaces.trafo
                        toEffect DefaultSurfaces.vertexColor
                        toEffect DefaultSurfaces.simpleLighting                              
                        ]
                    |> Sg.noEvents
                )
            textarea [ style "position: fixed; top: 5px; left: 5px"; onChange SetText ] m.text
            br[]
            button [ style "position: fixed; bottom: 5px; right: 5px"; onClick (fun () -> ToggleVR) ] text
            br []
            button [ style "position: fixed; bottom: 30px; right: 5px"; onClick (fun () -> AddBox) ] textAddBox
            br []
            textarea [ style "position: fixed; bottom: 55px; right: 5px"; onChange SetText] distanceToBox 
              
        ]

    let ui' (info : VrSystemInfo) (m : MModel) = 
        let text = m.vr |> Mod.map (function true -> "Stop VR" | false -> "Start VR")

        let opcs = 
            m.opcInfos
              |> AMap.toASet
              |> ASet.map(fun info -> Sg.createSingleOpcSg m.opcAttributes.selectedScalar (Mod.constant false) m.cameraState.view info)
              |> Sg.set
              |> Sg.effect [ 
                toEffect Shader.stableTrafo
                toEffect DefaultSurfaces.diffuseTexture  
                toEffect Shader.AttributeShader.falseColorLegend //falseColorLegendGray
                ]
              
        let frustum =
            Mod.constant (Frustum.perspective 60.0 0.1 100.0 1.0)

        div [ style "width: 100%; height: 100%" ] [
            FreeFlyController.controlledControl m.cameraState CameraMessage m.mainFrustum
                (AttributeMap.ofList [
                    style "width: 100%; height:100%"; 
                    attribute "showFPS" "true";       // optional, default is false
                    attribute "useMapping" "true"
                    attribute "data-renderalways" "false"
                    attribute "data-samples" "4"
                ])
                (
                    opcs
                    //|> Sg.map (OpcSelectionViewer.Message.PickingAction) 
                    |> Sg.map OpcViewerMsg
                    |> Sg.noEvents
                )
            button [ style "position: fixed; bottom: 5px; right: 5px"; onClick (fun () -> ToggleVR) ] text
        ]
    
    let vr (info : VrSystemInfo) (m : MModel) =

        let color = Mod.constant C4b.Green

        let a = 
            Sg.box' C4b.Cyan Box3d.Unit
            |> Sg.noEvents
            |> Sg.scale 0.01
            |> Sg.trafo (m.ControllerPosition |> Mod.map (fun current -> Trafo3d.Translation(current)))
        

        let deviceSgs = 
            info.state.devices |> AMap.toASet |> ASet.chooseM (fun (_,d) ->
                d.Model |> Mod.map (fun m ->
                    match m with
                    | Some sg -> 
                        sg 
                        |> Sg.noEvents 
                        |> Sg.trafo d.pose.deviceToWorld
                        |> Sg.onOff d.pose.isValid
                        |> Some
                    | None -> 
                        None 
                )
            )
            |> Sg.set
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.diffuseTexture
                do! DefaultSurfaces.simpleLighting
            }

        m.mainMenuBoxes 
        |> AList.toASet 
        |> ASet.map (fun b -> 
            mkISg m b 
           )
        |> Sg.set
        |> Sg.andAlso a  
        |> Sg.effect [
            toEffect DefaultSurfaces.trafo
            toEffect DefaultSurfaces.vertexColor
            toEffect DefaultSurfaces.simpleLighting                              
            ]
        //Sg.textWithConfig TextConfig.Default m.text
        |> Sg.noEvents
        |> Sg.andAlso deviceSgs

    let vr' (info : VrSystemInfo) (m : MModel)= 

        let points = m.drawingPoint |> AList.toMod

        let lines = 
            points |> Mod.map (fun list -> 
                let l = PList.toArray list |> Array.map (fun box -> box)
                l
                |> Array.pairwise
                |> Array.map (fun (a,b) -> new Line3d(a.trafo.GetValue().GetModelOrigin(),b.trafo.GetValue().GetModelOrigin()))                
            )

        let drawLines = 
            lines 
                |> Sg.lines (Mod.constant C4b.White)
                |> Sg.noEvents
                |> Sg.uniform "LineWidth" (Mod.constant 5) 
                |> Sg.effect [
                    toEffect DefaultSurfaces.trafo
                    toEffect DefaultSurfaces.vertexColor
                    toEffect DefaultSurfaces.thickLine
                    ]
                |> Sg.pass (RenderPass.after "lines" RenderPassOrder.Arbitrary RenderPass.main)
                |> Sg.depthTest (Mod.constant DepthTestMode.None)
            
        let a = 
            m.controllerInfos
            |> AMap.toASet
            |> ASet.map (fun boxController -> 
                let ci_pose = snd boxController
                mkControllerBox ci_pose.pose)
            |> Sg.set
            |> Sg.effect [
                toEffect DefaultSurfaces.trafo
                toEffect DefaultSurfaces.vertexColor
                toEffect DefaultSurfaces.simpleLighting                              
                ]

        //let menuBox = 
        //    m.mainMenuBoxes
        //    |> AList.toASet 
        //    |> ASet.map (fun b -> 
        //        mkISg m b 
        //       )
        //    |> Sg.set
        //    |> Sg.effect [
        //        toEffect DefaultSurfaces.trafo
        //        toEffect DefaultSurfaces.vertexColor
        //        toEffect DefaultSurfaces.simpleLighting                              
        //        ]
        //    |> Sg.noEvents

        //let annotationSubMenuBox = 
        //    m.subMenuBoxes
        //    |> AList.toASet 
        //    |> ASet.map (fun b -> 
        //        mkISg m b 
        //        )
        //    |> Sg.set
        //    |> Sg.effect [
        //        toEffect DefaultSurfaces.trafo
        //        toEffect DefaultSurfaces.vertexColor
        //        toEffect DefaultSurfaces.simpleLighting                              
        //        ]
        //    |> Sg.noEvents

        let deviceSgs = 
            info.state.devices |> AMap.toASet |> ASet.chooseM (fun (_,d) ->
                d.Model |> Mod.map (fun m ->
                    match m with
                    | Some sg -> 
                        sg 
                        |> Sg.noEvents 
                        |> Sg.trafo d.pose.deviceToWorld
                        |> Sg.onOff d.pose.isValid
                        |> Some
                    | None -> 
                        None 
                )
            )
            |> Sg.set
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.diffuseTexture
                do! DefaultSurfaces.simpleLighting
            }
    
        let opcs = 
            m.opcInfos
                |> AMap.toASet
                |> ASet.map(fun info -> 
                    Sg.createSingleOpcSg m.opcAttributes.selectedScalar (Mod.constant false) m.cameraState.view info
                    )
                |> Sg.set
                |> Sg.effect [ 
                    toEffect Shader.stableTrafo
                    toEffect DefaultSurfaces.diffuseTexture  
                    toEffect Shader.AttributeShader.falseColorLegend
                ]
                |> Sg.noEvents

        opcs
        |> Sg.map OpcViewerMsg
        |> Sg.noEvents
        |> Sg.trafo m.globalTrafo 
        |> Sg.andAlso deviceSgs
        |> Sg.andAlso a
        //|> Sg.andAlso menuBox
        //|> Sg.andAlso annotationSubMenuBox
        //|> Sg.andAlso preDrawingBoxes
        |> Sg.andAlso drawLines
          
        //let boxTest = 
        //    Sg.box (Mod.constant C4b.Red) (Mod.constant Box3d.Unit)
        //        |> Sg.shader {
        //            do! DefaultSurfaces.trafo
        //            do! DefaultSurfaces.vertexColor
        //            do! DefaultSurfaces.simpleLighting
        //            }
        //        |> Sg.noEvents
        //        |> Sg.trafo m.globalTrafo   
        //boxTest 
        //|> Sg.noEvents
        //|> Sg.andAlso deviceSgs
        //|> Sg.andAlso a
        //|> Sg.andAlso menuBox
        //|> Sg.andAlso annotationBoxes
   
    let pause (info : VrSystemInfo) (m : MModel) =
        Sg.box' C4b.Red Box3d.Unit
        |> Sg.noEvents
        |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! DefaultSurfaces.vertexColor
            do! DefaultSurfaces.simpleLighting
        }

    let initial =
        let rotateBoxInit = false
        let patchHierarchiesInit = 
            OpcViewerFunc.patchHierarchiesImport "C:\Users\lopez\Desktop\GardenCity\MSL_Mastcam_Sol_929_id_48423"

        let boundingBoxInit = 
            OpcViewerFunc.boxImport (patchHierarchiesInit)

        let opcInfosInit = 
            OpcViewerFunc.opcInfosImport (patchHierarchiesInit)

        let up =
            OpcViewerFunc.getUpVector boundingBoxInit rotateBoxInit

        let upRotationTrafo = 
            Trafo3d.RotateInto(boundingBoxInit.Center.Normalized, V3d.OOI)

        let cameraStateInit = 
            OpcViewerFunc.restoreCamStateImport boundingBoxInit V3d.OOI
        {
            text                = "some text"
            vr                  = false
            mainMenuBoxes       = PList.empty
            boxHovered          = None
            subMenuBoxes        = PList.empty

            ControllerPosition      = V3d.OOO
            controllerInfos         = HMap.empty
            offsetToCenter          = V3d.One
            cameraState             = cameraStateInit
            
            patchHierarchies    = patchHierarchiesInit
            boundingBox         = boundingBoxInit
            opcInfos            = opcInfosInit
            opcAttributes       = SurfaceAttributes.initModel "C:\Users\lopez\Desktop\GardenCity\MSL_Mastcam_Sol_929_id_48423"
            mainFrustum         = Frustum.perspective 60.0 0.01 1000.0 1.0
            rotateBox           = rotateBoxInit
            pickingModel        = OpcViewer.Base.Picking.PickingModel.initial

            globalTrafo         = Trafo3d.Translation(-boundingBoxInit.Center) * upRotationTrafo //global trafo for opc, with center in boundingbox center

            offsetControllerDistance = 1.0

            initGlobalTrafo     = Trafo3d.Identity
            initControlTrafo    = Trafo3d.Identity
            init2ControlTrafo   = Trafo3d.Identity
            rotationAxis        = Trafo3d.Identity

            menu                    = MenuState.Navigation
            controllerMenuSelector  = ControllerKind.ControllerA
            annotationMenu          = subMenuState.Draw
            initialMenuState        = MenuState.Annotation
            menuButtonPressed       = false
            initialMenuPosition     = Pose.none
            initialMenuPositionBool = false
            drawingPoint            = PList.empty
        }
    let app =
        {
            unpersist = Unpersist.instance
            initial = initial
            update = update
            threads = threads
            input = input 
            ui = ui'
            vr = vr'
            pauseScene = Some pause
        }

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

type DemoMessage =
    | SetText of string 
    | ToggleVR
    | CreateMenu of int*bool
    | Select of string
    | HoverIn of string
    | HoverOut
    | CameraMessage    of FreeFlyController.Message    
    | SetControllerPosition of int*Pose
    | GrabObject of int*int*bool
    | TranslateObject of V3d
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


    let rec update (state : VrState) (vr : VrActions) (model : Model) (msg : DemoMessage) : Model=
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
        | CreateMenu (controllerIndex, buttonPressed) ->
            let model = 
                if not(model.initialMenuPositionBool) then 
                    let controllerPos = model.controllerPositions |> HMap.values |> Seq.item controllerIndex
                    {model with initialMenuPosition = controllerPos.pose; initialMenuPositionBool = true}
                else model
            if buttonPressed then 
                let hmdPos = model.controllerPositions |> HMap.values |> Seq.item 0
                match model.menu with
                | Navigation ->
                    let newMenuBoxes = OpcUtilities.mkBoxesMenu model.initialMenuPosition hmdPos.pose 2 //number of menu possibilities should be the number of boxes. So far 2
                    let box0id = newMenuBoxes |> Seq.item 0
                    let newMenuBoxes = 
                        newMenuBoxes 
                        |> PList.map (fun idx -> 
                            if idx.id.Equals(box0id.id) then {idx with id = "Navigation"}
                            else {idx with id = "Annotation"}
                            )
                    {model with boxes = newMenuBoxes; menuButtonPressed = buttonPressed}
                | Annotation -> 
                    let newSubMenuBoxes = OpcUtilities.mkBoxesMenu model.initialMenuPosition hmdPos.pose 4
                    let boxID0 = newSubMenuBoxes |> Seq.item 0
                    let boxID1 = newSubMenuBoxes |> Seq.item 1 
                    let boxID2 = newSubMenuBoxes |> Seq.item 2
                    let newSubMenuBoxes = 
                        newSubMenuBoxes
                        |> PList.map (fun idx -> 
                            if idx.id.Equals(boxID0.id)then {idx with id = "Back"}
                            else if idx.id.Equals(boxID1.id) then {idx with id = "Dip and Strike"}
                            else if idx.id.Equals(boxID2.id) then {idx with id = "Flag"}
                            else {idx with id = "Line"})
                    {model with subMenuBoxes = newSubMenuBoxes; menuButtonPressed = buttonPressed}
            else 
                {model with boxes = PList.empty; subMenuBoxes = PList.empty; menuButtonPressed = buttonPressed; initialMenuPositionBool = false}
            
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
        | SetControllerPosition (controllerIndex, p) -> 
            let newModel = 
                match model.menu with
                | MenuState.Navigation ->
                    model 
                    |> NavigationOpc.currentSceneInfo controllerIndex p
                | MenuState.Annotation ->
                    let newModel = 
                        model
                        |> AnnotationOpc.annotationMode controllerIndex p model.annotationMenu
                    newModel

            // store cnotrollers positions in a new variable
            let newModel = 
                if newModel.controllerPositions.Count.Equals(5) then
                    let controller1, controller2 = 
                        newModel 
                        |> OpcUtilities.getControllersInfo 3 4

                    let mayHoverMenu = OpcUtilities.mayHover newModel.boxes controller1 controller2

                    let newModel = 
                        match mayHoverMenu with 
                        | Some ID -> 
                            if controller2.joystickPressed || controller1.joystickPressed then
                                let boxID = newModel.boxes |> Seq.item 0
                                let menuSelector = 
                                    if controller2.joystickPressed then newModel.controllerPositions |> HMap.keys |> Seq.item 4
                                    else newModel.controllerPositions |> HMap.keys |> Seq.item 3

                                if boxID.id.Contains(ID) then 
                                    {newModel with menu = MenuState.Navigation}
                                else 
                                    {newModel with menu = MenuState.Annotation; controllerMenuSelector = menuSelector;boxes = PList.empty}
                            else update state vr newModel (HoverIn ID)
                        | None -> update state vr newModel HoverOut
                    newModel 
                else newModel

            let newModel = 
                if newModel.menu.Equals(MenuState.Annotation) && newModel.controllerPositions.Count.Equals(5) then 
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
                            
                            if boxID0.id.Contains(ID) then {newModel with menu = MenuState.Navigation}
                            else if boxID1.id.Contains(ID) then {newModel with annotationMenu = AnnotationMenuState.Flag}
                            else if boxID2.id.Contains(ID) then{newModel with annotationMenu = AnnotationMenuState.DipAndStrike}
                            else {newModel with annotationMenu = AnnotationMenuState.Line}
                        else update state vr newModel (HoverIn ID)
                    | None -> update state vr newModel HoverOut
                else {newModel with subMenuBoxes = PList.empty}
            newModel
            
        | GrabObject (controllerIndex, buttonPressed, buttonPress)->
            
            printfn "Menu mode is: %s when buttonpress is: %s" (model.menu.ToString()) (buttonPress.ToString())
            
            let updateControllerButtons = 
                model.controllerPositions
                |> HMap.alter controllerIndex (fun but ->  
                match but with
                | Some x -> 
                    match buttonPressed with 
                    | 0 -> Some {x with joystickPressed = buttonPress}
                    | 1 -> Some {x with backButtonPressed = buttonPress}
                | None -> 
                    match buttonPressed with 
                    | 1 -> 
                        let newInfo = {
                            pose = Pose.none
                            //buttons  = ButtonStates
                            frontButtonPressed = false
                            backButtonPressed = buttonPress
                            joystickPressed = false
                         }
                        Some newInfo
                    | 0 -> 
                        let newInfo = {
                            pose = Pose.none
                            //buttons  = ButtonStates
                            frontButtonPressed = false
                            backButtonPressed = false
                            joystickPressed = buttonPress
                         }
                        Some newInfo)

            let newModel = {model with controllerPositions = updateControllerButtons; initialMenuState = model.menu; controllerMenuSelector = controllerIndex}
            
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
                let controllerPos = newModel.controllerPositions |> HMap.values |> Seq.item controllerIndex
                let newFlag = VisibleBox.createFlag (C4b.Magenta) (controllerPos.pose.deviceToWorld.GetModelOrigin())
                newModel 
                
        | _ -> model

    let mkColor (model : MModel) (box : MVisibleBox) =
        let id = box.id

        let color = 
            id
            |> Mod.bind (fun s ->
                let selectedColor =
                    model.boxSelected
                    |> ASet.contains s
                    |> Mod.bind(function 
                        | true -> Mod.constant C4b.White
                        | false -> box.color
                    )

                let hoverColor =
                    model.boxHovered 
                    |> Mod.bind (function 
                        | Some k -> if k = s then Mod.constant C4b.Blue else selectedColor
                        | None -> selectedColor
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
                Sg.text font C4b.White box.id//(Mod.constant "text")
                    |> Sg.noEvents
                    |> Sg.trafo(Mod.constant(Trafo3d.RotationInDegrees(V3d(90.0,0.0,90.0))))
                    |> Sg.scale 0.05
                    |> Sg.trafo(pos)
                    |> Sg.pickable (PickShape.Box (box1))
            )
                |> Sg.dynamic 
        
        let menuBox = 
            Sg.box color box.geometry
            //Sg.wireBox color box.geometry
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

    let threads (model : Model) =
        ThreadPool.empty
        
    let input (msg : VrMessage) =
        match msg with
        // buttons identifications: sensitive = 0, backButton = 1, sideButtons = 2
        | VrMessage.Touch(con,button) -> 
            match button with 
            | 0 -> [CreateMenu(con, true)]
            | _ -> []
        | VrMessage.Untouch(con,button) -> 
            match button with 
            | 0 -> [CreateMenu(con, false)]
            | _ -> []
        | VrMessage.PressButton(_,2) ->
            //printfn "Button identification %d" button
            [ToggleVR]
        | VrMessage.UpdatePose(cn,p) -> 
            if p.isValid then 
                let pos = p.deviceToWorld.Forward.TransformPos(V3d.Zero)
                //printfn "%d changed pos= %A" cn pos
                [SetControllerPosition (cn, p)]
            else []
        | VrMessage.Press(con,button) -> 
            printfn "%d Button identification %d" con button
            match button with
            //| 0 -> [CreateMenu(con, true)]
            | _ -> [GrabObject(con, button, true)]
        | VrMessage.Unpress(con,button) -> 
            printfn "Button unpressed by %d" con
            match button with 
            //| 0 -> [CreateMenu(con, false)]
            | _ -> [GrabObject (con, button, false)]
        | _ -> 
            []


    let mkControllerBox (cp : MPose) =
        Sg.box' C4b.Cyan Box3d.Unit
        //Sg.cone' 1 C4b.Cyan 5.0 10.0 
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
        
        let line1 = 
            Sg.lines (Mod.constant C4b.Green) m.lines
            |> Sg.noEvents
            |> Sg.uniform "LineWidth" (Mod.constant 2.0)
            |> Sg.effect [
                toEffect DefaultSurfaces.stableTrafo
                toEffect DefaultSurfaces.thickLine
            ]

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
                    m.boxes 
                        |> AList.toASet 
                        |> ASet.map (function b -> mkISg m b)
                        |> Sg.set
                        |> Sg.effect [
                            toEffect DefaultSurfaces.trafo
                            toEffect DefaultSurfaces.vertexColor
                            toEffect DefaultSurfaces.simpleLighting                              
                            ]
                        |> Sg.noEvents
                        |> Sg.andAlso line1
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

        //let line = 
        //    adaptive{
        //        let! startPoint = m.startingLinePos
        //        let! endPoint = m.endingLinePos
        //        let newLine : Line3d = Line3d(startPoint, endPoint)
        //        return [| newLine |]
        //    }

        let line1 = 
            Sg.lines color m.lines
            |> Sg.noEvents
            |> Sg.uniform "LineWidth" (Mod.constant 2.0)
            |> Sg.effect [
                toEffect DefaultSurfaces.stableTrafo
                toEffect DefaultSurfaces.thickLine
            ]
            //|> Sg.trafo
            


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

        m.boxes 
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
        |> Sg.andAlso line1

    let vr' (info : VrSystemInfo) (m : MModel)= 

        let a = 
            m.controllerPositions
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

        let menuBox = 
            m.boxes
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
        |> Sg.andAlso menuBox
        |> Sg.andAlso annotationSubMenuBox
          
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

    let newBoxList = PList.empty//OpcUtilities.mkBoxes 2
    let newSubMenuBoxList = PList.empty
    
    //let patchHierarchiesDir = Directory.GetDirectories("C:\Users\lopez\Desktop\20190717_VictoriaCrater\VictoriaCrater_HiRISE") |> Array.head |> Array.singleton

    let initial =
        let rotateBoxInit = true
        let PatchHierarchiesInit = 
            OpcViewerFunc.patchHierarchiesImport "C:\Users\lopez\Desktop\GardenCity\MSL_Mastcam_Sol_929_id_48423"
        let BoundingBoxInit = 
            OpcViewerFunc.boxImport (PatchHierarchiesInit)
        let OpcInfosInit = 
            OpcViewerFunc.opcInfosImport (PatchHierarchiesInit)
        let upInit =
            OpcViewerFunc.upImport BoundingBoxInit rotateBoxInit
        let CameraStateInit = 
            OpcViewerFunc.restoreCamStateImport BoundingBoxInit upInit
        {
            text = "some text"
            vr = false
            boxes = newBoxList
            boxHovered = None
            boxSelected = HSet.empty
            subMenuBoxes = newSubMenuBoxList

            ControllerPosition = V3d.OOO
            grabbed = HSet.empty
            controllerPositions = HMap.empty
            isPressed = false
            offsetToCenter = V3d.One
            boxDistance = V3d.Zero
            startingLinePos = V3d.Zero
            endingLinePos = V3d.Zero
            lines = [||]
            cameraState = CameraStateInit
            
            patchHierarchies = PatchHierarchiesInit
            boundingBox = BoundingBoxInit
            opcInfos = OpcInfosInit
            opcAttributes = SurfaceAttributes.initModel "C:\Users\lopez\Desktop\GardenCity\MSL_Mastcam_Sol_929_id_48423"
            mainFrustum = Frustum.perspective 60.0 0.01 1000.0 1.0
            rotateBox = rotateBoxInit
            pickingModel = OpcViewer.Base.Picking.PickingModel.initial
            controllerDistance = 1.0
            globalTrafo = Trafo3d.Translation -BoundingBoxInit.Center //global trafo for opc, with center in boundingbox center
            offsetControllerDistance = 1.0
            initGlobalTrafo = Trafo3d.Identity
            initControlTrafo = Trafo3d.Identity
            init2ControlTrafo = Trafo3d.Identity
            rotationAxis = Trafo3d.Identity
            menu = MenuState.Navigation
            controllerMenuSelector = 0
            annotationMenu = AnnotationMenuState.Flag
            initialMenuState = MenuState.Navigation
            menuButtonPressed = false
            initialMenuPosition = Pose.none
            initialMenuPositionBool = false
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

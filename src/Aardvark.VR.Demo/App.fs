namespace Demo.Main

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
open Demo.Menu
open Demo


type DemoAction =
| SetText of string 
| ToggleVR
| MenuMessage of MenuAction * ControllerKind * bool
| CameraMessage         of FreeFlyController.Message    
| SetControllerPosition of ControllerKind *  Pose
| GrabObject            of ControllerKind * ControllerButtons * bool
| OpcViewerMsg of PickingAction
| GetTrackpadPosition of ControllerKind * int * V2d

module Demo =
    open Aardvark.Application //TODO ML: avoid using module opens ... just put them up in one place
    open Aardvark.VRVis.Opc
    open Aardvark.UI.Primitives
    open Aardvark.Base.Rendering
    open Model
    open OpenTK
    open Valve.VR
    open OpenTK.Input
    open Aardvark.UI.Extensions
    open OpcViewer.Base.Shader
    
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


    let rec update (state : VrState) (vr : VrActions) (model : Model) (msg : DemoAction) : Model =
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
        | MenuMessage (a, kind, buttonTouched) ->   
            
            let updateCont = 
                model.controllerInfos 
                |> HMap.alter kind (fun old -> 
                    match old, buttonTouched with 
                    | Some x, true -> 
                        Some { x with joystickHold = true }   // update / overwrite
                    | Some x, false -> 
                        Some { x with joystickHold = false }   // update / overwrite
                    | None, true -> 
                        Some 
                            {ControllerInfo.initial with 
                                kind            = kind
                                buttonKind      = ControllerButtons.Joystick
                                joystickHold    = true 
                            } //create
                    | None, false -> 
                        Some
                            {ControllerInfo.initial with 
                                kind        = kind
                                buttonKind  = ControllerButtons.Joystick 
                            } //create
                )

            let newModel = {model with controllerInfos = updateCont}

            let controllers = newModel.controllerInfos

            let newMenuModel = 
                let checkCon = controllers |> HMap.tryFind kind
                match checkCon with 
                | Some controller -> 
                    if not (controller.sideButtonPressed) then 
                        MenuApp.update controllers state vr newModel.menuModel a
                    else newModel.menuModel
                | None -> newModel.menuModel
            
            { newModel with 
                menuModel = newMenuModel; 
            }

        | CameraMessage m -> 
            { model with cameraState = FreeFlyController.update model.cameraState m }   
        | SetControllerPosition (kind, p) ->    
            let newModel = 
                match model.menuModel.menu with
                | Menu.MenuState.Navigation ->
                    model 
                    |> NavigationOpc.currentSceneInfo kind p
                | Menu.MenuState.Annotation (annotationMenu)  ->
                    model
                    |> AnnotationOpc.annotationMode kind p annotationMenu
                | Menu.MenuState.MainReset -> 
                    Model.initMainReset
            
            let controllerMenuUpdate = MenuApp.update model.controllerInfos state vr newModel.menuModel (MenuAction.UpdateControllerPose (kind, p))

            let someLineHovered = 
                    newModel.finishedLine
                    |> HMap.map (fun idPoints fl -> 
                        fl.points |> PList.filter(fun lp -> lp.hovered = true)
                    )
                    |> HMap.values
                    |> Seq.first
            
            let newLineIsHovered = 
                match someLineHovered with 
                | Some h -> 
                    let getHovered = 
                        h
                        |> PList.tryFirst
                    match getHovered with 
                    | Some s -> 
                        s.hovered
                    | None -> false
                | None -> false

            let someFlagHover = 
                newModel.flagOnAnnotationSpace
                |> PList.filter (fun f -> f.flagHovered = true)
                |> Seq.first

            let newFlagIsHovered = 
                match someFlagHover with 
                | Some f -> 
                    f.flagHovered
                | None -> false

            let newModel = 
                {newModel with 
                    menuModel = controllerMenuUpdate; 
                    lineIsHovered = newLineIsHovered; 
                    flagIsHovered = newFlagIsHovered
                }
            
            let changeLineMode = 
                if newModel.lineIsHovered then 
                    {newModel.menuModel with lineSubMenu = LineSubMenuState.EditLine}
                else {newModel.menuModel with lineSubMenu = LineSubMenuState.LineCreate}//; menu = MenuState.Annotation (SubMenuState.Line LineSubMenuState.LineCreate)}
            
            let newModel = {newModel with menuModel = changeLineMode}

            let changeFlagMode = 
                if newModel.flagIsHovered then 
                    {newModel.menuModel with flagSubMenu = FlagSubMenuState.EditFlag; menu = MenuState.Annotation (SubMenuState.Flag FlagSubMenuState.EditFlag)}
                else {newModel.menuModel with flagSubMenu = FlagSubMenuState.FlagCreate}//; menu = MenuState.Annotation (SubMenuState.Flag FlagSubMenuState.FlagCreate)}
            
            {newModel with menuModel = changeFlagMode}
            //printfn "%s, %s" (newModel.lineIsHovered.ToString()) (newModel.menuModel.lineSubMenu.ToString())

            
        | GrabObject (kind, buttonKind, buttonPress)-> 
            
            printfn "Menu mode is: %s when buttonpress is: %s" (model.menuModel.menu.ToString()) (buttonPress.ToString())
            
            let updateControllerButtons = 
                model.controllerInfos
                |> HMap.alter kind (fun but ->  
                match but with
                | Some x -> 
                    match buttonKind with 
                    | ControllerButtons.Joystick -> Some {x with joystickPressed = buttonPress}
                    | ControllerButtons.Back -> Some {x with backButtonPressed = buttonPress}
                    | ControllerButtons.Side -> Some {x with sideButtonPressed = buttonPress}
                    | ControllerButtons.Home -> Some {x with homeButtonPressed = buttonPress}
                    | _ -> None
                    
                | None -> 
                    match buttonKind with 
                    | ControllerButtons.Side -> 
                        Some 
                            {
                                ControllerInfo.initial with 
                                    kind                = kind
                                    buttonKind          = buttonKind
                                    sideButtonPressed   = buttonPress
                            }
                    | ControllerButtons.Back -> 
                        Some 
                            {
                                ControllerInfo.initial with 
                                    kind                = kind 
                                    buttonKind          = buttonKind
                                    backButtonPressed   = buttonPress
                            }
                    | ControllerButtons.Joystick -> 
                        Some 
                            {
                                ControllerInfo.initial with 
                                    kind                = kind 
                                    buttonKind          = buttonKind
                                    joystickPressed     = buttonPress
                            }
                    | ControllerButtons.Home -> 
                        Some 
                            {
                                ControllerInfo.initial with
                                    kind                = kind
                                    buttonKind          = buttonKind
                                    homeButtonPressed   = buttonPress
                            }
                    | _ -> None
                )
            
            let newModel = {model with controllerInfos = updateControllerButtons}
            
            let homeButtonPressed = 
                match buttonKind with 
                | ControllerButtons.Home -> {newModel.menuModel with menu = MenuState.Navigation}
                | _ -> newModel.menuModel

            let newModel = {newModel with menuModel = homeButtonPressed}

            let controllerMenuUpdate = MenuApp.update newModel.controllerInfos state vr newModel.menuModel (MenuAction.Select (kind, buttonPress))

            let newModel = {newModel with menuModel = controllerMenuUpdate}

            match newModel.menuModel.menu with 
            | Navigation -> 
                let newModel = 
                    newModel
                    |> NavigationOpc.initialSceneInfo
                {newModel with 
                    flagOnController    = PList.empty
                    lineOnController    = PList.empty
                    dipAndStrikeOnController = PList.empty
                }
            | Annotation (annotationMenu) -> //TODO ML: make your own annotation app
                let controllerPos = newModel.controllerInfos |> HMap.tryFind kind
                match controllerPos with 
                | Some id -> 
                    match annotationMenu with
                    | Draw -> 
                        match buttonKind with 
                        | ControllerButtons.Back -> 
                            match buttonPress with 
                            | true -> 
                                match newModel.currentlyDrawing with 
                                | Some v -> 
                                    let newPolygon =   
                                        v.vertices
                                        |> PList.prepend (id.pose.deviceToWorld.GetModelOrigin() * newModel.workSpaceTrafo.Inverse.GetModelOrigin())

                                    {newModel with currentlyDrawing = Some {vertices = newPolygon}}
                                | None -> 
                                    let newTrafoAnnotationSpace = id.pose.deviceToWorld * newModel.workSpaceTrafo.Inverse
                                    let newVectorAnnotationSpace = newTrafoAnnotationSpace.GetModelOrigin()
                                    {newModel with currentlyDrawing = Some {vertices = newVectorAnnotationSpace |> PList.single}}
                            | false -> 
                                //printfn "New polygon created"
                                //match newModel.currentlyDrawing with 
                                //| None -> newModel 
                                //| Some p -> 
                                //    let newFinishedPol = 
                                //        newModel.finishedDrawings
                                //        |> HMap.add (System.Guid.NewGuid().ToString()) p
                                        
                                //    {newModel with finishedDrawings = newFinishedPol; currentlyDrawing = Some {vertices = [||]}}
                                newModel
                        | _ -> newModel
                    | Flag (flagMenu)  -> 
                        let newFlag = OpcUtilities.mkFlags id.pose.deviceToWorld 1
                        {newModel with flagOnController = newFlag}
                    | Line (lineMenu)  -> 
                        match lineMenu with 
                        | LineSubMenuState.LineCreate -> 
                            let newLine = OpcUtilities.mkSphere id.pose 1 0.005
                            let newModel = {newModel with lineOnController = newLine}
                            printfn "button kind: %d" (buttonKind |> ControllerButtons.toInt)
                            match buttonKind with 
                            | ControllerButtons.Side -> 

                                let spherePoints = 
                                    newModel.lineOnAnnotationSpace
                                    |> PList.map (fun p -> 
                                        {
                                            pos         = p.trafo.GetModelOrigin()
                                            hovered     = false 
                                            color       = C4b.White
                                        }
                                    )

                                let newFinishedLine = 
                                    newModel.finishedLine
                                    |> HMap.alter (System.Guid.NewGuid().ToString()) (fun fl ->
                                        match fl with 
                                        | Some updateLine -> Some updateLine //update line... never happens
                                        | None -> // create new finished line
                                            let newLine = 
                                                {
                                                    points          = spherePoints
                                                    trafo           = Trafo3d.Identity
                                                    colorLine       = C4b.White
                                                    colorVertices   = C4b.White
                                                    //finishedLineOnMars      = newModel.lineOnMars
                                                    //finishedLineMarsDisplay = newModel.lineMarsDisplay
                                                }
                                            Some newLine
                                    )

                                let newModel = {newModel with finishedLine = newFinishedLine}

                                let newControllerLine = 
                                    OpcUtilities.mkSphere id.pose 1 0.005

                                {newModel with 
                                    lineOnController = newControllerLine;
                                    lineOnAnnotationSpace = PList.empty
                                }    
                                //newModel
                            | _ -> newModel 
                        | LineSubMenuState.EditLine -> 
                            printfn "new MOOOOOOODE"
                            {newModel with lineOnController = PList.empty}
                    | DipAndStrike -> 
                        let indexDS = 
                            newModel.dipAndStrikeOnController
                            |> PList.tryFirstIndex
                        let newCylinder = 
                            match indexDS with 
                            | Some idx ->
                                newModel.dipAndStrikeOnController
                                |> PList.alter idx (fun disk ->
                                    match disk with 
                                    | Some x -> 
                                        match id.joystickPressed with 
                                        | true -> 
                                            Some {x with radius = x.radius}
                                        | false -> Some {x with radius = x.radius}
                                    | None -> Some VisibleCylinder.initial
                                )
                            | None -> OpcUtilities.mkCyllinder id.pose 1 0.5

                        {newModel with dipAndStrikeOnController = newCylinder}
                    | _ -> newModel
                | None -> newModel
            | MainReset -> 
                Model.initMainReset
        
        | GetTrackpadPosition (con, axis, pos) -> 
            let controller = model.controllerInfos |> HMap.tryFind con
            match controller with 
            | Some c -> 
                match model.menuModel.menu with 
                | Annotation (annotationMenu)  -> 
                    match annotationMenu with 
                    | DipAndStrike -> 
                        match c.sideButtonPressed, c.joystickPressed with 
                        | true, true ->
                        //if c.sideButtonPressed && c.joystickPressed then 
                            match pos.Y with 
                            | x when x >= 0.60 -> 
                                printfn "make radius bigger"
                                let newDipAndStrikeOnController =
                                    model.dipAndStrikeOnController
                                    |> PList.map (fun disk -> 
                                        printfn "%f" disk.radius
                                        {disk with radius = disk.radius + 0.01}
                                    )
                                {model with dipAndStrikeOnController = newDipAndStrikeOnController}
                            | x when x < -0.40 -> 
                                printfn "make radius smaller"
                                let newDipAndStrikeOnController = 
                                    model.dipAndStrikeOnController
                                    |> PList.map (fun disk ->
                                        if disk.radius <= 0.0 then 
                                            {disk with radius = 0.0}
                                        else {disk with radius = disk.radius - 0.01}
                                    )
                                {model with dipAndStrikeOnController = newDipAndStrikeOnController}
                            | _ -> model
                        | _ -> model//else model
                    | _ -> model
                | _ -> model 

            | None -> model 

            
                
    let mkColor (model : MModel) (box : MVisibleBox) =
        let id = box.id

        let color = 
            id
            |> Mod.bind (fun s ->

                let hoverColor =
                    model.menuModel.boxHovered 
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
            | 0 -> [MenuMessage (Demo.MenuAction.CreateMenu(con |> ControllerKind.fromInt, true), con |> ControllerKind.fromInt, true)]
            | _ -> []
        | VrMessage.Untouch(con,button) -> 
            match button with 
            | 0 -> [MenuMessage (Demo.MenuAction.CreateMenu(con |> ControllerKind.fromInt, false), con |> ControllerKind.fromInt, false)]
            | _ -> []
        | VrMessage.ValueChange(con, axis, pos) ->
            match axis with 
            | 0 -> 
                [GetTrackpadPosition(con |> ControllerKind.fromInt, axis, pos)]
            | _ -> []
        | VrMessage.PressButton(con,button) ->
            printfn "button pressed: %d" button
            match button with 
            | 2 -> [GrabObject(con |> ControllerKind.fromInt, button |> ControllerButtons.fromInt, true)]
            | 1 -> [GrabObject(con |> ControllerKind.fromInt, 3 |> ControllerButtons.fromInt, true)]
            | _ -> []
        | VrMessage.UnpressButton(con, button) -> 
            match button with 
            | 2 -> [GrabObject(con |> ControllerKind.fromInt, button |> ControllerButtons.fromInt, false)]
            | _ -> []
        | VrMessage.UpdatePose(cn,p) -> 
            if p.isValid then 
                [SetControllerPosition (cn |> ControllerKind.fromInt, p)]
            else []
        | VrMessage.Press(con,button) -> 
            printfn "%d Touch identification %d" con button
            match button with
            | _ -> [GrabObject(con |> ControllerKind.fromInt, button |> ControllerButtons.fromInt, true)]
        | VrMessage.Unpress(con,button) -> 
            printfn "Touch unpressed by %d" con
            match button with 
            | _ -> [GrabObject (con |> ControllerKind.fromInt, button |> ControllerButtons.fromInt, false)]
        | _ -> 
            []


    let mkControllerBox (cp : MPose) =
        Sg.cone' 20 C4b.Cyan 0.5 5.0 
            |> Sg.noEvents
            |> Sg.scale 0.01
            |> Sg.trafo (Mod.constant (Trafo3d.RotationInDegrees(V3d(-90.0,90.0,0.0))))
            |> Sg.trafo cp.deviceToWorld
    
    let mkFlag (model : MModel) (box : MVisibleBox) =
        let color = mkColor model box
        let pos = box.trafo

        Sg.box color box.geometry
            |> Sg.noEvents
            |> Sg.trafo(pos)
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.vertexColor
                //do! DefaultSurfaces.simpleLighting
                }      
                
    let mkSphere (model : MModel) (sphere : MVisibleSphere) =
        let color = sphere.color
        let pos = sphere.trafo

        Sg.sphere 10 color sphere.radius
            |> Sg.noEvents
            |> Sg.trafo(pos)
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.vertexColor
                //do! DefaultSurfaces.simpleLighting
                }  

    let mkCylinder (model : MModel) (cylinder : MVisibleCylinder) = 
        let pos = cylinder.trafo
        let color = 
            adaptive {
                let! dipAngle = model.dipAndStrikeAngle
                let! min = Mod.constant 0.0
                let! max = Mod.constant 90.0

                let range = new Range1d(min, max)
                let hue = (dipAngle - range.Min) / range.Size
                let hsv = HSVf((1.0 - hue) * 0.625, 1.0, 1.0)
                return hsv.ToC3f().ToC4b()
            }
        let rad = 
            adaptive { 
                let! rad1 = cylinder.radius
                return rad1
            }

        Sg.cylinder 20 color rad (Mod.constant 0.01)
            |> Sg.noEvents
            |> Sg.trafo(pos)
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.vertexColor
                //do! DefaultSurfaces.simpleLighting
            }

    let mkCylinderOnAnnotationSpace (model : MModel) (cylinder : MVisibleCylinder) = 
        let color = cylinder.color
        let pos = cylinder.trafo

        Sg.cylinder 20 color cylinder.radius (Mod.constant 0.01)
            |> Sg.noEvents
            |> Sg.trafo(pos)
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.vertexColor
                //do! DefaultSurfaces.simpleLighting
            }

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

        let flags = 
            m.flagOnController
            |> AList.toASet 
            |> ASet.map (fun b -> 
                mkFlag m b 
               )
            |> Sg.set
            |> Sg.effect [
                toEffect DefaultSurfaces.trafo
                toEffect DefaultSurfaces.vertexColor
                toEffect DefaultSurfaces.simpleLighting                              
                ]
            |> Sg.noEvents

        let flagsOnMars = 
            m.flagOnAnnotationSpace
            |> AList.toASet 
            |> ASet.map (fun b -> 
                mkFlag m b 
               )
            |> Sg.set
            |> Sg.effect [
                toEffect DefaultSurfaces.trafo
                toEffect DefaultSurfaces.vertexColor
                toEffect DefaultSurfaces.simpleLighting                              
                ]
            |> Sg.noEvents
              
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
                    |> Sg.map OpcViewerMsg
                    |> Sg.noEvents
                    |> Sg.andAlso flags
                    |> Sg.andAlso flagsOnMars
                )
            button [ style "position: fixed; bottom: 5px; right: 5px"; onClick (fun () -> ToggleVR) ] text
        ]
    
    let vr' (info : VrSystemInfo) (m : MModel) = 

        let drawLineToPolygon = 
            m.currentlyDrawing
            |> Mod.bind (fun cd -> 
                match cd with 
                | Some p -> 
                    p.vertices
                    |> AList.toMod 
                    |> Mod.map (fun v ->
                        if v.IsEmpty() then
                            [|Line3d(V3d.OOO,V3d.III)|]
                        else
                            v
                            |> PList.toArray
                            |> Array.pairwise
                            |> Array.map (fun (a, b) -> Line3d(a, b))
                    )
                | None -> Mod.constant [| Line3d() |]
            )

        let drawPolygon =
            drawLineToPolygon
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

        //let drawFinishedPolygon = 
        //    m.finishedDrawings
        //    |> AMap.toASet
        //    |> ASet.map(fun (_, polygon) -> 
        //        let newPolygonLine = 
        //            polygon.vertices
        //            |> Mod.map (fun v -> 
        //                v
        //                |> Array.pairwise
        //                |> Array.map (fun (a,b) -> new Line3d(a, b))
        //            )
        //        newPolygonLine 
        //        |> Sg.lines (Mod.constant C4b.White)
        //        |> Sg.noEvents
        //        |> Sg.uniform "LineWidth" (Mod.constant 5) 
        //        |> Sg.effect [
        //            toEffect DefaultSurfaces.trafo
        //            toEffect DefaultSurfaces.vertexColor
        //            toEffect DefaultSurfaces.thickLine
        //            ]
        //        |> Sg.pass (RenderPass.after "lines" RenderPassOrder.Arbitrary RenderPass.main)
        //        |> Sg.depthTest (Mod.constant DepthTestMode.None)
        //    )
        //    |> Sg.set

        let flags = 
            m.flagOnController
            |> AList.toASet 
            |> ASet.map (fun b -> 
                mkFlag m b 
               )
            |> Sg.set
            |> Sg.effect [
                toEffect DefaultSurfaces.trafo
                toEffect DefaultSurfaces.vertexColor
                toEffect DefaultSurfaces.simpleLighting                              
                ]
            |> Sg.noEvents

        let spheres = 
            m.lineOnController
            |> AList.toASet
            |> ASet.map (fun b -> 
                mkSphere m b 
               )
            |> Sg.set
            |> Sg.effect [
                toEffect DefaultSurfaces.trafo
                toEffect DefaultSurfaces.vertexColor
                toEffect DefaultSurfaces.simpleLighting                              
                ]
            |> Sg.noEvents

        let sphereOnAnnotationSpace = 
            m.lineOnAnnotationSpace
            |> AList.toASet
            |> ASet.map (fun b -> 
                mkSphere m b 
               )
            |> Sg.set
            |> Sg.effect [
                toEffect DefaultSurfaces.trafo
                toEffect DefaultSurfaces.vertexColor
                toEffect DefaultSurfaces.simpleLighting                              
                ]
            |> Sg.noEvents

        let drawinALine = 
            let ttt = 
                m.lineOnAnnotationSpace
                |> AList.toMod
                |> Mod.map (fun l -> 
                        l
                        |> PList.tryFirst
                    )
            let t2 = 
                m.lineOnAnnotationSpace
                |> AList.toMod
                |> Mod.map (fun l -> 
                    l 
                    |> PList.tryAt 1
                )
            adaptive {
                let! qwer = ttt
                let! asdf = t2
                match qwer with 
                | Some point1 -> 
                    match asdf with 
                    | Some point2 -> 
                        return [|Line3d(point1.trafo.GetValue().GetModelOrigin(), point2.trafo.GetValue().GetModelOrigin())|]
                    | None -> return [|Line3d()|]
                | None -> return [|Line3d()|]
            }

        let drawLineTest = 
            drawinALine 
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

        let drawSphereLines = 
            m.lineMarsDisplay
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
                
        let dynamicLine = 
            let conLine = 
                m.lineOnController
                |> AList.toMod
                |> Mod.map (fun cl -> 
                    cl
                    |> PList.tryFirst
                )
            let conLine1 = 
                conLine 
                |> Mod.bind (fun sphere -> 
                    match sphere with 
                    | Some id -> id.trafo 
                    | None -> Mod.constant (Trafo3d.Translation(V3d(-50000.0, -50000.0, -50000.0)))
                )
                
            let marsLine = 
                m.lineOnAnnotationSpace
                |> AList.toMod
                |> Mod.map (fun ml -> 
                    ml
                    |> PList.tryFirst
                )
            let marsLine1 = 
                marsLine 
                |> Mod.bind (fun sphere -> 
                    match sphere with 
                    | Some id -> id.trafo 
                    | None -> Mod.constant (Trafo3d.Translation(V3d(50000.0, 50000.0, 50000.0)))
                )
            adaptive {
                let! conLineTest = conLine |> Mod.map (fun tt -> tt)
                let! marsLineTest = marsLine  |> Mod.map (fun tt -> tt)
                
                match conLineTest with 
                | Some clID -> 
                    match marsLineTest with 
                    | Some mlID -> 
                        let! conLineTrafo = conLine1 |> Mod.map (fun t -> t )
                        let! marsLineTrafo = marsLine1 |> Mod.map (fun t -> t )
                        return [|Line3d(conLineTrafo.GetModelOrigin(), marsLineTrafo.GetModelOrigin())|]
                    | None -> return [|Line3d()|]
                | None -> return [|Line3d()|]
            }

        let showDynamicLine = 
            dynamicLine
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

        let font = Font.create "Consolas" FontStyle.Regular

        let distanceText =
            m.lineOnAnnotationSpace
            |> AList.toASet
            |> ASet.map (fun dist -> //dist.trafo, dist.distance
                let distTrafo = dist.trafo |> Mod.map (fun dt -> dt * Trafo3d.Translation(V3d(0.0, 0.0, 0.05)))
                Sg.textWithConfig { TextConfig.Default with renderStyle = RenderStyle.Billboard; align = TextAlignment.Center; flipViewDependent = true } dist.distance
                |> Sg.noEvents
                |> Sg.scale 0.05
                |> Sg.trafo(distTrafo)
            )
            |> Sg.set

        let dsAngleText = 
            m.dipAndStrikeOnAnnotationSpace
            |> AList.toASet
            |> ASet.map (fun angle -> 
                let angleTrafo = angle.trafo |> Mod.map (fun at -> at * Trafo3d.Translation(V3d(0.0, 0.0, 0.05)))
                Sg.textWithConfig { TextConfig.Default with renderStyle = RenderStyle.Billboard; align = TextAlignment.Center; flipViewDependent = true } angle.angle
                |> Sg.noEvents
                |> Sg.scale 0.05
                |> Sg.trafo(angleTrafo)
            )
            |> Sg.set

        let flagsOnAnnotationSpace = 
            m.flagOnAnnotationSpace
            |> AList.toASet 
            |> ASet.map (fun b -> 
                mkFlag m b 
               )
            |> Sg.set
            |> Sg.effect [
                toEffect DefaultSurfaces.trafo
                toEffect DefaultSurfaces.vertexColor
                toEffect DefaultSurfaces.simpleLighting                              
                ]
            |> Sg.noEvents

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

        let finishedLineHmap = 
            m.finishedLine
            |> AMap.toASet
            |> ASet.map (fun (s, b) ->
                let text = 
                    b.points
                    |> AList.toList
                    |> List.pairwise
                    |> List.map (fun (a, b) -> 
                        let distance = Mod.constant (V3d.Distance(a.pos, b.pos))
                        let distance = distance |> Mod.map (fun d -> System.Math.Round(d, 3).ToString())
                        Sg.textWithConfig { TextConfig.Default with renderStyle = RenderStyle.Billboard; align = TextAlignment.Center; flipViewDependent = true } distance
                        |> Sg.noEvents
                        |> Sg.scale 0.05
                        |> Sg.trafo(Mod.constant (Trafo3d.Translation(a.pos + V3d(0.0, 0.0, 0.05))))
                    )
                    |> Sg.ofList


                let newFinishedLine = 
                    b.points
                    |> AList.toList
                    |> List.pairwise
                    |> List.map (fun (a, b) -> Line3d(a.pos, b.pos))
                    |> List.toArray

                let renderFinishedLine = 
                    Mod.constant newFinishedLine
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

                let newFinishedLineOnMars = 
                    b.points
                    |> AList.map (fun l -> 
                        let newSphere = Mod.constant (VisibleSphere.createSphere l.color l.pos 0.005)
                        let color = newSphere |> Mod.map (fun s -> s.color)
                        let pos = newSphere |> Mod.map (fun s -> s.trafo)
                        Sg.sphere 10 color (newSphere |> Mod.map (fun s -> s.radius))
                        |> Sg.noEvents
                        |> Sg.trafo(pos)
                        |> Sg.shader {
                            do! DefaultSurfaces.trafo
                            do! DefaultSurfaces.vertexColor
                            //do! DefaultSurfaces.simpleLighting
                            }  
                    )                
                    |> AList.toASet
                    |> Sg.set
                    |> Sg.effect [
                        toEffect DefaultSurfaces.trafo
                        toEffect DefaultSurfaces.vertexColor
                        toEffect DefaultSurfaces.simpleLighting                              
                        ]
                    |> Sg.noEvents
                
                newFinishedLineOnMars
                |> Sg.andAlso renderFinishedLine
                |> Sg.andAlso text
                
            )
            |> Sg.set

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
    
        let cylinders = 
            m.dipAndStrikeOnController
            |> AList.toASet
            |> ASet.map (fun c -> 
                mkCylinder m c
            )
            |> Sg.set
            |> Sg.effect [
                toEffect DefaultSurfaces.trafo
                toEffect DefaultSurfaces.vertexColor
                toEffect DefaultSurfaces.simpleLighting                              
                ]
            |> Sg.noEvents

        let cylindersOnAnnotationSpace = 
            m.dipAndStrikeOnAnnotationSpace
            |> AList.toASet
            |> ASet.map (fun c -> 
                mkCylinderOnAnnotationSpace m c
            )
            |> Sg.set
            |> Sg.effect [
                toEffect DefaultSurfaces.trafo
                toEffect DefaultSurfaces.vertexColor
                toEffect DefaultSurfaces.simpleLighting                              
                ]
            |> Sg.noEvents
        
        let menuApp = 
            MenuApp.vr info m.menuModel
            |> Sg.map MenuMessage

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
                |> Sg.map OpcViewerMsg
                |> Sg.noEvents     
                |> Sg.trafo m.opcSpaceTrafo
        
        let transformedSgs =    
            [
                drawPolygon
                //drawFinishedPolygon
                flagsOnAnnotationSpace
                sphereOnAnnotationSpace
                drawLineTest
                //drawSphereLines
                distanceText
                dsAngleText
                finishedLineHmap
                cylinders
                cylindersOnAnnotationSpace
            ]   
            |> Sg.ofList
            |> Sg.trafo m.annotationSpaceTrafo

        let inMenuDisappear = 
            [
                flags
                spheres
                showDynamicLine
                //cylinders
            ]   
            |> Sg.ofList

        let mkDisappear = 
            let controllerInfo = m.menuModel.controllerMenuSelector
            let controllerKind = controllerInfo.kind
            let newCP = 
                controllerKind |> Mod.bind (fun ck -> 
                    m.controllerInfos |> AMap.tryFind ck)
                
            let shouldShow = 
                adaptive {
                    let! newConPos = newCP
                    match newConPos with
                        | None -> return false
                        | Some e -> 
                            let! isPressed = e.joystickHold
                            return not isPressed
                }
            inMenuDisappear
            |> Sg.onOff shouldShow

        let notTransformedSgs =
            [
                deviceSgs
                a
                menuApp
            ] |> Sg.ofList

        Sg.ofList [transformedSgs; notTransformedSgs; mkDisappear; opcs]

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
            OpcViewerFunc.patchHierarchiesImport "C:\Users\lopez\Desktop\VictoriaCrater\HiRISE_VictoriaCrater_SuperResolution"

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

            ControllerPosition      = V3d.OOO
            controllerInfos         = HMap.empty
            offsetToCenter          = V3d.One
            cameraState             = cameraStateInit
            
            patchHierarchies    = patchHierarchiesInit
            boundingBox         = boundingBoxInit
            opcInfos            = opcInfosInit
            opcAttributes       = SurfaceAttributes.initModel "C:\Users\lopez\Desktop\VictoriaCrater\HiRISE_VictoriaCrater_SuperResolution"
            mainFrustum         = Frustum.perspective 60.0 0.01 1000.0 1.0
            rotateBox           = rotateBoxInit
            pickingModel        = OpcViewer.Base.Picking.PickingModel.initial

            offsetControllerDistance    = 1.0

            opcSpaceTrafo               = Trafo3d.Translation(-boundingBoxInit.Center) * upRotationTrafo
            workSpaceTrafo              = Trafo3d.Identity
            annotationSpaceTrafo              = Trafo3d.Identity

            initOpcSpaceTrafo           = Trafo3d.Translation(-boundingBoxInit.Center) * upRotationTrafo
            initWorkSpaceTrafo          = Trafo3d.Identity
            initAnnotationSpaceTrafo          = Trafo3d.Identity

            initControlTrafo            = Trafo3d.Identity
            init2ControlTrafo           = Trafo3d.Identity
            rotationAxis                = Trafo3d.Identity

            menuModel               = Menu.MenuModel.init
            drawingPoint            = PList.empty
            drawingLine             = [|Line3d()|]

            currentlyDrawing        = None
            finishedDrawings        = HMap.empty

            flagOnController        = PList.empty
            flagOnAnnotationSpace              = PList.empty
            lineOnController        = PList.empty
            lineOnAnnotationSpace              = PList.empty
            lineMarsDisplay         = [|Line3d()|]
            finishedLine            = HMap.empty
            lineIsHovered           = false
            flagIsHovered           = false
            dipAndStrikeOnController        = PList.empty
            dipAndStrikeOnAnnotationSpace   = PList.empty
            dipAndStrikeAngle               = 0.0
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

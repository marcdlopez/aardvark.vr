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
    | Select of string
    | HoverIn of string
    | HoverOut
    | CameraMessage    of FreeFlyController.Message    
    | SetControllerPosition of int*Pose
    | GrabObject of int*bool
    | TranslateObject of V3d
    | AddBox
    | OpcViewerMsg of PickingAction

module Demo =
    open Aardvark.Application
    open Aardvark.VRVis.Opc
    open Aardvark.UI.Primitives
    open Aardvark.Base.Rendering
    open Model
    
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

    let mkBoxes (number: int) : plist<VisibleBox> =        
        [0..number-1]
        |> List.map (fun x -> VisibleBox.createVisibleBox C4b.Red (V3d(0.0, 2.0 * float x, 0.0)))
        |> PList.ofList

    let getWorldTrafoIfBackPressed index model : Option<Trafo3d> = 
        let i0 = (model.controllerPositions |> HMap.keys |> Seq.item index)
        let b0 = model.controllerPositions |> HMap.tryFind i0
        b0
        |> Option.bind(fun x -> 
            match x.backButtonPressed with
            | true -> Some x.pose.deviceToWorld
            | false -> None)
        //|> Option.defaultValue model.initGlobalTrafo
        

    let rec update (state : VrState) (vr : VrActions) (model : Model) (msg : DemoMessage) : Model=
        match msg with
        | OpcViewerMsg m -> 
            let newOpcModel = OpcViewer.Base.Picking.PickingApp.update OpcViewer.Base.Picking.PickingModel.initial m
            
            {model with pickingModel = newOpcModel}
            //{ model with opcModel = newOpcModel }
        | SetText t -> 
            { model with text = t }
        | ToggleVR ->
            if model.vr then vr.stop()
            else vr.start()
            { model with vr = not model.vr }
        | HoverIn id ->
            match model.boxHovered with 
            | Some oldID when id = oldID -> model
            | _ ->
                Log.warn "Entered box with ID: %A" id
                { model with boxHovered = Some id} 
        | HoverOut ->
            if model.boxHovered.IsSome then
                Log.warn "Exit box with ID: %A" model.boxHovered.Value    
                { model with boxHovered = None}
            else 
                Log.warn "Nothing hovered"
                model
        | CameraMessage m -> 
            { model with cameraState = FreeFlyController.update model.cameraState m }   
        | SetControllerPosition (controllerIndex, p) -> 
            let newControllersPosition = 
                //if model.controllerPositions.ContainsKey(controllerIndex) then
                model.controllerPositions |> HMap.alter controllerIndex (fun old -> 
                match old with 
                | Some x -> 
                    Some {x with pose = p}   // update / overwrite
                | None -> 
                    let newInfo = {
                       pose = p
                       //buttons  = ButtonStates.
                       backButtonPressed = false
                       frontButtonPressed = false
                    }
                    Some  newInfo) // creation 
                
            let newModel = { model with controllerPositions = newControllersPosition}

            let newModel : Model = 
                let controllersFiltered = 
                    newModel.controllerPositions
                    |> HMap.filter (fun index CI -> 
                        CI.backButtonPressed = true
                    )
                match controllersFiltered.Count with
                | 1 ->
                    let i0 = (newModel.controllerPositions |> HMap.keys |> Seq.item controllerIndex)
                    let b0 = newModel.controllerPositions |> HMap.find i0
                    if b0.backButtonPressed  then
                        let shitftVecDevice = b0.pose.deviceToWorld.GetModelOrigin() - newModel.initControlTrafo.GetModelOrigin()
                        let newTrafoShift = newModel.initGlobalTrafo * (Trafo3d.Translation shitftVecDevice) 
                        printfn "%A" (newTrafoShift.GetModelOrigin())
                        {newModel with globalTrafo = newTrafoShift}
                    else newModel
                | 2 ->
                    let i0 = controllersFiltered |> HMap.keys |> Seq.item 0
                    let i1 = controllersFiltered |> HMap.keys |> Seq.item 1
                    let b0 = newModel.controllerPositions |> HMap.find i0
                    let b1 = newModel.controllerPositions |> HMap.find i1
                    let v0 = b0.pose.deviceToWorld.Forward.TransformPos(V3d.Zero)
                    let v1 = b1.pose.deviceToWorld.Forward.TransformPos(V3d.Zero)
                    let newControllerDistance = V3d.Distance(v0, v1) - newModel.offsetControllerDistance + newModel.initGlobalTrafo.GetScale()
                    //printfn "Distance between Controllers: %f" newControllerDistance
                    let test = newModel.initGlobalTrafo * Trafo3d.Scale (newControllerDistance) 
                    printfn "New Position of opc SCALE TEST: %A" (test.GetScale())
                    printfn "New Position of opc SCALE CONTROL TRAFO: %A" (newModel.initControlTrafo.GetScale())
                    let testtt = test.GetScale()
                    {newModel with controllerDistance = newControllerDistance; globalTrafo = test}
                | _ -> 
                    newModel
            
            newModel

            //if newModel.controllerButtons.Count=2 then 
            //    let idx1 = newModel.controllerButtons |> HMap.keys
            //    let i0 = (idx1 |> Seq.item 0)
            //    let i1 = (idx1 |> Seq.item 1)
            //    let b0 = newModel.controllerButtons |> HMap.find i0
            //    let b1 = newModel.controllerButtons |> HMap.find i1
            //    if b0 && b1 then 
            //        //store position of the controllers to calculate distance between them
            //        let p0 = model.controllerPositions |> HMap.find i0
            //        let p1 = model.controllerPositions |> HMap.find i1
            //        let v0 = p0.deviceToWorld.Forward.TransformPos(V3d.Zero)
            //        let v1 = p1.deviceToWorld.Forward.TransformPos(V3d.Zero)
            //        let newControllerDistance = V3d.Distance(v0, v1) - newModel.offsetControllerDistance + newModel.initGlobalTrafo.GetScale()
            //        printfn "Distance between Controllers: %f" newControllerDistance
            //        {newModel with controllerDistance = newControllerDistance; globalTrafo = Trafo3d.Scale (newControllerDistance)}
            //    else
            //        newModel
            //elif newModel.controllerButtons.Count = 1 then 
            //    let idx1 = newModel.controllerButtons |> HMap.keys
            //    let i0 = (idx1 |> Seq.item 0)
            //    let b0 = newModel.controllerButtons |> HMap.find i0
            //    if b0  then
            //        let cp0 = (newModel.controllerPositions |> HMap.find (newModel.controllerButtons |> HMap.keys |> Seq.item 0))
            //        let shitftVecDevice = cp0.deviceToWorld.GetModelOrigin() - newModel.initControlTrafo.GetModelOrigin()
            //        let newTrafoShift = newModel.initGlobalTrafo * (Trafo3d.Translation shitftVecDevice) 
            //        printfn "%A" (newTrafoShift.GetModelOrigin())
            //        {newModel with globalTrafo = newTrafoShift}
            //    else newModel
            //else newModel
            
            //let newModel = { model with ControllerPosition = p }

            //let mayHover = 
            //    newModel.boxes 
            //    |> PList.choose (fun b ->
            //        if b.geometry.Transformed(b.trafo).Contains(p) then
            //            Some b.id
            //        else None)
            //    |> PList.tryFirst

            //let newModel = 
            //    match mayHover with
            //    | Some ID -> update state vr newModel (HoverIn ID)
            //    | None -> update state vr newModel HoverOut
            
            //let newModel = 
            //    if newModel.isPressed then 
            //        match newModel.boxHovered with
            //        | Some ID -> 
            //            let moveBox = 
            //                newModel.boxes 
            //                |> PList.toList 
            //                |> List.tryPick (fun x -> if x.id = ID then Some x else None)
            //            match moveBox with
            //            | Some b -> 
            //                let index = 
            //                    newModel.boxes
            //                    |> PList.findIndex b
            //                let newBoxList = 
            //                    newModel.boxes 
            //                    |> PList.alter index (fun x -> x |> Option.map (fun y -> 
            //                        Log.line "update position to %A" newModel.ControllerPosition
            //                        { y with trafo = Trafo3d.Translation(newModel.ControllerPosition + newModel.offsetToCenter)}))
            //                { newModel with boxes = newBoxList }
            //            | None -> newModel
            //        | None -> newModel
            //    else newModel

            //let lines = [|
            //    for i in newModel.boxes do 
            //        for j in newModel.boxes do 
            //            if i.id != j.id then 
            //                let startPos = i.trafo.GetModelOrigin()
            //                let endPos = j.trafo.GetModelOrigin()
            //                printfn "Distance between boxes: %f" (V3d.Distance(startPos, endPos))
            //                yield (Line3d [startPos; endPos])
            //|]

            ////printfn "Bounding box position (opc): %A" newModel.opcModel.boundingBox.Center
            ////printfn "Camera position: %A " newModel.ControllerPosition

            //{ newModel with lines = lines }
            
        | GrabObject (controllerIndex, buttonPress)->

            let updateControllerButtons = 
                model.controllerPositions
                |> HMap.alter controllerIndex (fun but ->  
                match but with
                | Some x -> Some {x with backButtonPressed = buttonPress}
                | None -> 
                    let newInfo = {
                        pose = Pose.none
                        //buttons  = ButtonStates.
                        backButtonPressed = buttonPress
                        frontButtonPressed = false
                     }
                    Some newInfo)

            let model = {model with controllerPositions = updateControllerButtons}

            let newTrafo, InitialControllerDistance = 
                let controllersFiltered = 
                    model.controllerPositions
                    |> HMap.filter (fun index CI -> 
                        CI.backButtonPressed = true
                        )
                printfn "Buttons pressed at the same time: %i" controllersFiltered.Count
                match controllersFiltered.Count with
                | 1 -> 
                    let test = 
                        model 
                        |> getWorldTrafoIfBackPressed controllerIndex 
                        //|> Option.defaultValue model.initGlobalTrafo
                    match test with 
                    | Some x -> x, model.offsetControllerDistance
                    | None -> model.initGlobalTrafo, model.offsetControllerDistance
                | 2 -> 
                    let i0 = (controllersFiltered |> HMap.keys |> Seq.item 0)
                    let i1 = (controllersFiltered |> HMap.keys |> Seq.item 1)
                    let b0 = model.controllerPositions |> HMap.find i0
                    let b1 = model.controllerPositions |> HMap.find i1
                    printfn " All buttons true" 
                    let v1 = b0.pose.deviceToWorld.GetModelOrigin()
                    let v2 = b1.pose.deviceToWorld.Forward.TransformPos(V3d.Zero)
                    let dist = V3d.Distance(v1, v2)
                    
                    model.initGlobalTrafo, dist//Trafo3d.Scale (model.controllerDistance), dist
                | _ -> 
                    model.initGlobalTrafo, model.offsetControllerDistance
            
            let model = {model with initGlobalTrafo = model.globalTrafo; initControlTrafo = newTrafo; offsetControllerDistance = InitialControllerDistance}

            //let newOffsetControllerDist = 
            //    if model.controllerButtons.Count=2 then 
            //        let idx1 = model.controllerButtons |> HMap.keys
            //        let i0 = (idx1 |> Seq.item 0)
            //        let i1 = (idx1 |> Seq.item 1)
            //        let b0 = model.controllerButtons |> HMap.find i0
            //        let b1 = model.controllerButtons |> HMap.find i1
            //        if b0 && b1 then 
            //            //store position of the controllers to calculate distance between them
            //            let p0 = model.controllerPositions |> HMap.find i0
            //            let p1 = model.controllerPositions |> HMap.find i1
            //            let v1 = p0.deviceToWorld.Forward.TransformPos(V3d.Zero)
            //            let v2 = p1.deviceToWorld.Forward.TransformPos(V3d.Zero)
            //            V3d.Distance(v1, v2)
            //            //let dist = V3d.Distance(v1, v2)
            //            //Trafo3d.Scale (dist)
            //        else 
            //            //model.initGlobalTrafo
            //            model.offsetControllerDistance                   
            //    else
            //        //model.initGlobalTrafo
            //        model.offsetControllerDistance
           
            //let model = {model with offsetControllerDistance = newOffsetControllerDist; initGlobalTrafo = Trafo3d.Scale (model.controllerDistance)}

            //let initControlShift = 
            //    if model.controllerButtons.Count = 1 then 
            //        let idx1 = model.controllerButtons |> HMap.keys
            //        let i0 = (idx1 |> Seq.item 0)
            //        //let i1 = (idx1 |> Seq.item 1)
            //        let b0 = model.controllerButtons |> HMap.find i0
            //        //let b1 = model.controllerButtons |> HMap.find i1
            //        if b0  then 
            //            //store position of the controllers to calculate distance between them
            //            let p0 = model.controllerPositions |> HMap.find i0
            //            //let p1 = model.controllerPositions |> HMap.find i1
            //            let v1 = p0.deviceToWorld.GetModelOrigin()//.Forward.TransformPos(V3d.Zero)
            //            //let bboundCenterInOriginSpace = model.globalTrafo.Forward.TransformPos(model.boundingBox.Center)
            //            ////let v2 = p1.deviceToWorld.Forward.TransformPos(V3d.Zero)
            //            //printfn "v1 : %A" v1
            //            //printfn "bboundingBox: %A" bboundCenterInOriginSpace
            //            //Trafo3d.Translation (bboundCenterInOriginSpace - v1)
            //            p0.deviceToWorld
            //        else model.initControlTrafo
            //    else model.initControlTrafo

            //let model = {model with initGlobalTrafo = model.globalTrafo; initControlTrafo = initControlShift}

            let offset = 
                model.boxes 
                |> PList.choose (fun b ->
                    if b.geometry.Transformed(b.trafo).Contains(model.ControllerPosition) && buttonPress then
                        printfn "Offset to the origin: %A " (b.trafo.GetModelOrigin() - model.ControllerPosition)
                        Some (b.trafo.GetModelOrigin() - model.ControllerPosition)
                    else 
                        None
                )
                |> PList.tryFirst
                |> Option.defaultValue V3d.Zero
            
            { model with isPressed = buttonPress; offsetToCenter = offset}
            
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
        Sg.box color box.geometry
            //|> Sg.transform (Trafo3d.Translation pos)
            |> Sg.trafo(pos)
            //|> Sg.scale 0.25
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.vertexColor
                do! DefaultSurfaces.simpleLighting
                }                
            |> Sg.requirePicking
            |> Sg.noEvents
            |> Sg.withEvents [
                //Sg.onClick (fun _  -> GrabObject true)
                Sg.onEnter (fun _  -> HoverIn  (box.id.ToString()))
                Sg.onLeave (fun _ -> HoverOut)
            ]

    let threads (model : Model) =
        ThreadPool.empty
        
    let input (msg : VrMessage)=
        match msg with
        | VrMessage.PressButton(_,_) ->
            [ToggleVR]
        | VrMessage.UpdatePose(cn,p) -> 
            if p.isValid then 
                let pos = p.deviceToWorld.Forward.TransformPos(V3d.Zero)
                //printfn "%d changed pos= %A"  0 pos
                [SetControllerPosition (cn, p)]
            else []
        | VrMessage.Press(con,_) -> 
            printfn "Button pressed by %d" con
            [GrabObject(con, true)]
        | VrMessage.Unpress(con,_) -> 
            printfn "Button unpressed by %d" con
            [GrabObject (con, false)]
        | _ -> 
            []


    let mkControllerBox (cp : Pose) =
        Sg.box' C4b.Cyan Box3d.Unit
            |> Sg.noEvents
            |> Sg.scale 0.01
            |> Sg.trafo (Mod.constant cp.deviceToWorld) //|> Mod.map (fun current -> Trafo3d.Translation(current))

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
                let test = snd boxController
                mkControllerBox test.pose)
                //boxController)//mkControllerBox (snd boxController ))
            |> Sg.set
            |> Sg.effect [
                toEffect DefaultSurfaces.trafo
                toEffect DefaultSurfaces.vertexColor
                toEffect DefaultSurfaces.simpleLighting                              
                ]

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
            
        //let myComplexTrafo (m:MModel)=
        //    adaptive {
        //        let! bb = m.boundingBox 
        //        let! shift = m.controllerPositions.Content
        //        let! c0Shift = shift |> HMap.tryFind 0 |> Option.map(fun x -> x.deviceToWorld) |> Option.defaultValue (Mod.constant Trafo3d.Identity)
        //        //let! c1Shift = shift |> HMap.tryFind 1 |> Option.map(fun x -> x.deviceToWorld) |> Option.defaultValue (Mod.constant Trafo3d.Identity)
        //        return Trafo3d.Translation(bb.Center) * c0Shift// * c1Shift
        //    }

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
                    toEffect Shader.AttributeShader.falseColorLegend //falseColorLegendGray
                ]
                |> Sg.noEvents
                //|> Sg.trafo (m.globalTrafo)
                //|> Sg.translate' (m.boundingBox |> Mod.map (fun p -> - p.Center))
                
        opcs
        //|> Sg.map (OpcSelectionViewer.Message.PickingAction)
        //|> Sg.map (OpcViewer.Base.Picking.PickingAction.HitSurface)
        |> Sg.map OpcViewerMsg
        |> Sg.noEvents
        |> Sg.trafo m.globalTrafo //(m.controllerDistance |> Mod.map(fun f -> Trafo3d.Scale (max 0.01 f+1.0)))
        |> Sg.andAlso deviceSgs
        |> Sg.andAlso a
   
    let pause (info : VrSystemInfo) (m : MModel) =
        Sg.box' C4b.Red Box3d.Unit
        |> Sg.noEvents
        |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! DefaultSurfaces.vertexColor
            do! DefaultSurfaces.simpleLighting
        }

    let newBoxList = mkBoxes 2
    
    let patchHierarchiesDir = Directory.GetDirectories("C:\Users\lopez\Desktop\GardenCity\MSL_Mastcam_Sol_929_id_48423") |> Array.head |> Array.singleton

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
            //cameraState = FreeFlyController.initial
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
            
            //opcModel = OpcSelectionViewer.App.createBasicModel "C:\Users\lopez\Desktop\GardenCity\MSL_Mastcam_Sol_929_id_48423" None true
            patchHierarchies = PatchHierarchiesInit
            boundingBox = BoundingBoxInit
            opcInfos = OpcInfosInit
            opcAttributes = SurfaceAttributes.initModel "C:\Users\lopez\Desktop\GardenCity\MSL_Mastcam_Sol_929_id_48423"
            mainFrustum = Frustum.perspective 60.0 0.01 1000.0 1.0
            rotateBox = rotateBoxInit
            pickingModel = OpcViewer.Base.Picking.PickingModel.initial
            //controllerButtons = hmap.Empty
            controllerDistance = 1.0
            globalTrafo = Trafo3d.Translation -BoundingBoxInit.Center
            offsetControllerDistance = 1.0
            initGlobalTrafo = Trafo3d.Identity
            initControlTrafo = Trafo3d.Identity
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

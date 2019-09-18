namespace Demo

open System
open System.IO
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.Rendering.Text
open Aardvark.Vr
open Aardvark.SceneGraph
open Aardvark.SceneGraph.Opc
open Aardvark.UI
open Aardvark.UI.Primitives
open Aardvark.UI.Trafos
open Aardvark.UI.Generic
open FShade
open Aardvark.Application.OpenVR

open OpcViewer.Base
open OpcViewer.Base.Picking
open OpcViewer.Base.Attributes
open Rabbyte.Drawing
open Rabbyte.Annotation

type Message =
    | SetText of string 
    | ToggleVR
    | Select of string
    | HoverIn of string
    | HoverOut
    | CameraMessage    of FreeFlyController.Message    
    | SetControllerPosition of V3d
    | GrabObject of bool
    | TranslateObject of V3d
    | AddBox

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

    let rec update (state : VrState) (vr : VrActions) (model : Model) (msg : Message) : Model=
        match msg with
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
        | SetControllerPosition p -> 
            let newModel = { model with ControllerPosition = p }

            let mayHover = 
                newModel.boxes 
                |> PList.choose (fun b ->
                    if b.geometry.Transformed(b.trafo).Contains(p) then
                        Some b.id
                    else None)
                |> PList.tryFirst

            let newModel = 
                match mayHover with
                | Some ID -> update state vr newModel (HoverIn ID)
                | None -> update state vr newModel HoverOut
            
            let newModel = 
                if newModel.isPressed then 
                    match newModel.boxHovered with
                    | Some ID -> 
                        let moveBox = 
                            newModel.boxes 
                            |> PList.toList 
                            |> List.tryPick (fun x -> if x.id = ID then Some x else None)

                        match moveBox with
                        | Some b -> 
                            let index = 
                                newModel.boxes
                                |> PList.findIndex b

                            let newBoxList = 
                                newModel.boxes 
                                |> PList.alter index (fun x -> x |> Option.map (fun y -> 
                                    Log.line "update position to %A" newModel.ControllerPosition
                                    { y with trafo = Trafo3d.Translation(newModel.ControllerPosition + newModel.offsetToCenter)}))
                            
                            { newModel with boxes = newBoxList }
                        | None -> newModel
                    | None -> newModel
                else newModel

            let lines = [|
                for i in newModel.boxes do 
                    for j in newModel.boxes do 
                        if i.id != j.id then 
                            let startPos = i.trafo.GetModelOrigin()
                            let endPos = j.trafo.GetModelOrigin()
                            printfn "Distance between boxes: %f" (V3d.Distance(startPos, endPos))
                            yield (Line3d [startPos; endPos])
            |]

            { newModel with lines = lines }
            
        | GrabObject buttonPress ->
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

        | AddBox -> 
            let newBoxList = 
                 model.boxes |> PList.append (VisibleBox.createVisibleBox C4b.DarkMagenta model.ControllerPosition)
            
            {model with boxes = newBoxList}
            
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
                Sg.onClick (fun _  -> GrabObject true)
                Sg.onEnter (fun _  -> HoverIn  (box.id.ToString()))
                Sg.onLeave (fun _ -> HoverOut)
            ]

    let threads (model : Model) =
        ThreadPool.empty
        
    let input (msg : VrMessage)=
        match msg with
        | VrMessage.PressButton(_,_) ->
            [ToggleVR]
            //[GrabObject]
        | VrMessage.UpdatePose(3,p) -> 
            if p.isValid then 
                let pos = p.deviceToWorld.Forward.TransformPos(V3d.Zero)
                //printfn "%d changed pos= %A"  0 pos
                [SetControllerPosition (pos)]
            else []
        | VrMessage.Press(con,_) -> 
            printfn "Button pressed by %d" con
            [GrabObject true]
        | VrMessage.Unpress(con,_) -> 
            printfn "Button unpressed by %d" con
            [GrabObject false]
        | _ -> 
            []

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
            m.opcModel.opcInfos
              |> AMap.toASet
              |> ASet.map(fun info -> Sg.createSingleOpcSg m.opcModel.opcAttributes.selectedScalar m.opcModel.pickingActive m.opcModel.cameraState.view info)
              |> Sg.set
              |> Sg.effect [ 
                toEffect Shader.stableTrafo
                toEffect DefaultSurfaces.diffuseTexture  
                toEffect Shader.AttributeShader.falseColorLegend //falseColorLegendGray
                ]
              //|> Sg.noEvents  
              

        div [ style "width: 100%; height: 100%" ] [
            FreeFlyController.controlledControl m.opcModel.cameraState CameraMessage frustum
                (AttributeMap.ofList [
                    attribute "style" "width:65%; height: 100%; float: left;"
                    attribute "data-samples" "8"
                ])
                (
                    opcs
                    |> Sg.map (OpcSelectionViewer.Message.PickingAction)
                    |> Sg.noEvents
                    
                    //m.boxes 
                    //    |> AList.toASet 
                    //    |> ASet.map (function b -> mkISg m b)
                    //    |> Sg.set
                    //    |> Sg.effect [
                    //        toEffect DefaultSurfaces.trafo
                    //        toEffect DefaultSurfaces.vertexColor
                    //        toEffect DefaultSurfaces.simpleLighting                              
                    //        ]
                    //    |> Sg.noEvents
                    //    |> Sg.andAlso line1
                )
            textarea [ style "position: fixed; top: 5px; left: 5px"; onChange SetText ] m.text
            br[]
            button [ style "position: fixed; bottom: 5px; right: 5px"; onClick (fun () -> ToggleVR) ] text
            //br []
            //button [ style "position: fixed; bottom: 30px; right: 5px"; onClick (fun () -> AddBox) ] textAddBox
            //br []
            //textarea [ style "position: fixed; bottom: 55px; right: 5px"; onChange SetText] distanceToBox 
              
        ]

    let ui' (info : VrSystemInfo) (m : MModel) = 

        let opcs = 
            m.opcModel.opcInfos
              |> AMap.toASet
              |> ASet.map(fun info -> Sg.createSingleOpcSg m.opcModel.opcAttributes.selectedScalar m.opcModel.pickingActive m.opcModel.cameraState.view info)
              |> Sg.set
              |> Sg.effect [ 
                toEffect Shader.stableTrafo
                toEffect DefaultSurfaces.diffuseTexture  
                toEffect Shader.AttributeShader.falseColorLegend //falseColorLegendGray
                ]
              
        let frustum =
            Mod.constant (Frustum.perspective 60.0 0.1 100.0 1.0)
        
        div [ style "width: 100%; height: 100%" ] [
            FreeFlyController.controlledControl m.opcModel.cameraState CameraMessage m.opcModel.mainFrustum
                (AttributeMap.ofList [
                    style "width: 100%; height:100%"; 
                    attribute "showFPS" "true";       // optional, default is false
                    attribute "useMapping" "true"
                    attribute "data-renderalways" "false"
                    attribute "data-samples" "4"
                ])
                (
                    opcs
                    |> Sg.map (OpcSelectionViewer.Message.PickingAction)
                    |> Sg.noEvents
                )
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
            m.opcModel.opcInfos
              |> AMap.toASet
              |> ASet.map(fun info -> Sg.createSingleOpcSg m.opcModel.opcAttributes.selectedScalar m.opcModel.pickingActive m.opcModel.cameraState.view info)
              |> Sg.set
              |> Sg.effect [ 
                toEffect Shader.stableTrafo
                toEffect DefaultSurfaces.diffuseTexture  
                toEffect Shader.AttributeShader.falseColorLegend //falseColorLegendGray
                ]
            |> Sg.requirePicking
            |> Sg.noEvents
            |> Sg.andAlso deviceSgs

        opcs
        
        //let renderControl =
        //    FreeFlyController.controlledControl m.opcModel.cameraState OpcSelectionViewer.Camera m.opcModel.mainFrustum
        //      (AttributeMap.ofList [ 
        //        style "width: 100%; height:100%"; 
        //        attribute "showFPS" "true";       // optional, default is false
        //        attribute "useMapping" "true"
        //        attribute "data-renderalways" "false"
        //        attribute "data-samples" "4"
        //        onKeyDown (OpcSelectionViewer.Message.KeyDown)
        //        onKeyUp (OpcSelectionViewer.Message.KeyUp)
        //        //onBlur (fun _ -> Camera FreeFlyController.Message.Blur)
        //      ]) 
        //      (MarsScene 
        //      |> Sg.requirePicking
        //      |> Sg.noEvents
        //      |> Sg.andAlso deviceSgs)

        //renderControl
     
    let pause (info : VrSystemInfo) (m : MModel) =
        Sg.box' C4b.Red Box3d.Unit
        |> Sg.noEvents
        |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! DefaultSurfaces.vertexColor
            do! DefaultSurfaces.simpleLighting
        }

    let newBoxList = mkBoxes 2
    
    let initial = 
        {
            text = "some text"
            vr = false
            boxes = newBoxList
            boxHovered = None
            boxSelected = HSet.empty
            cameraState = FreeFlyController.initial
            ControllerPosition = V3d.OOO
            grabbed = HSet.empty
            controllerPositions = HMap.empty
            isPressed = false
            offsetToCenter = V3d.One
            boxDistance = V3d.Zero
            startingLinePos = V3d.Zero
            endingLinePos = V3d.Zero
            lines = [||]
            opcModel = OpcSelectionViewer.App.createBasicModel "C:\Users\lopez\Desktop\GardenCity\MSL_Mastcam_Sol_929_id_48423" None true
        }
    let app =
        {
            unpersist = Unpersist.instance
            initial = initial
            update = update
            threads = threads
            input = input 
            ui = ui'
            vr = vr
            pauseScene = Some pause
        }

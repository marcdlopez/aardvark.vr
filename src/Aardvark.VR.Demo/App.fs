namespace Demo

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Rendering.Text
open Aardvark.Vr
open Aardvark.SceneGraph
open Aardvark.UI
open Aardvark.UI.Primitives
open Aardvark.UI.Generic
open Aardvark.Application.OpenVR
open System

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

    //let mkVisibleBox (color : C4b) (box : Box3d) (position : V3d) : VisibleBox = 
    //    {
    //        id = Guid.NewGuid().ToString()
    //        geometry = box
    //        color = color     
    //        //pose = pose
    //        trafo = Trafo3d.Translation(position)
    //        size = V3d.One
    //    }
     
    //let createUnitBox (center: V3d) : Box3d =
    //    Box3d.FromCenterAndSize(center, V3d.One)
    // Function to create box in a simple way

    //let mkNthBox i n = 
    //    Box3d.FromCenterAndSize(V3d.Zero, V3d.One)
       
     
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
            let newModel = { model with position = p }

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

            //newModel
            
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
                                    Log.line "update position to %A" newModel.position
                                    { y with trafo = Trafo3d.Translation(newModel.position - newModel.offsetToCenter)}))
                            
                            { newModel with boxes = newBoxList }
                        | None -> newModel
                    | None -> newModel
                else newModel
                
            newModel

        | GrabObject buttonPress ->
            let offset = 
                model.boxes 
                |> PList.choose (fun b ->
                    if b.geometry.Transformed(b.trafo).Contains(model.position) then
                        printfn "%A" (b.trafo.Forward.TransformPos(V3d.Zero))
                        Some (b.trafo.Forward.TransformPos(V3d.Zero))
                    else 
                        None
                )
                |> PList.tryFirst
                |> Option.defaultValue V3d.Zero
            
            
            { model with isPressed = buttonPress; offsetToCenter = offset}

            //let model =  { model with isPressed = newisPressed}

            //match model.boxHovered with
            //| Some ID -> 
            //    let newSelection = 
            //        if HSet.contains ID model.boxSelected then  
            //            HSet.remove ID model.boxSelected
            //        else HSet.add ID model.boxSelected

            //    { model with boxSelected = newSelection }
                //let model = { model with boxSelected = newSelection }

                //let moveBox = 
                //    model.boxes 
                //    |> PList.toList 
                //    |> List.tryPick (fun x -> if x.id = ID then Some x else None)

                //match moveBox with
                //| Some b -> 
                //    let index = 
                //        model.boxes
                //        |> PList.findIndex b

                //    let newBoxList = 
                //        model.boxes 
                //        |> PList.alter index (fun x -> x |> Option.map (fun y -> { y with trafo = Trafo3d.Translation model.position}))//Trafo3d.Translation model.position }))
                    
                //    { model with boxes = newBoxList }
                //| None ->
                //    model

            //| None -> model
        | AddBox -> 
            let newBoxList = 
                 model.boxes |> PList.append (VisibleBox.createVisibleBox C4b.DarkMagenta model.position)
            
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
        | VrMessage.UpdatePose(2,p) -> 
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
        
        div [ style "width: 100%; height: 100%" ] [
            FreeFlyController.controlledControl m.cameraState CameraMessage frustum
                (AttributeMap.ofList [
                    attribute "style" "width:65%; height: 100%; float: left;"
                    attribute "data-samples" "8"
                ])
                (
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
                )
            textarea [ style "position: fixed; top: 5px; left: 5px"; onChange SetText ] m.text
            br[]
            button [ style "position: fixed; bottom: 5px; right: 5px"; onClick (fun () -> ToggleVR) ] text
            br []
            button [ style "position: fixed; bottom: 30px; right: 5px"; onClick (fun () -> AddBox) ] textAddBox

        ]

    let vr (info : VrSystemInfo) (m : MModel) =

        let a = 
            Sg.box' C4b.Cyan Box3d.Unit
            |> Sg.noEvents
            |> Sg.scale 0.01
            |> Sg.trafo (m.position |> Mod.map (fun current -> Trafo3d.Translation(current)))
        

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

        
    let pause (info : VrSystemInfo) (m : MModel) =
        Sg.box' C4b.Red Box3d.Unit
        |> Sg.noEvents
        |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! DefaultSurfaces.vertexColor
            do! DefaultSurfaces.simpleLighting
        }

    let newBoxList = mkBoxes 3

    let initial = 
        {
            text = "some text"
            vr = false
            boxes = newBoxList
            boxHovered = None
            boxSelected = HSet.empty
            cameraState = FreeFlyController.initial
            position = V3d.OOO
            grabbed = HSet.empty
            controllerPositions = HMap.empty
            isPressed = false
            offsetToCenter = V3d.One
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

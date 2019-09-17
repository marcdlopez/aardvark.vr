namespace Rabbyte.Drawing

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Rabbyte.Drawing

[<AutoOpen>]
module Mutable =

    
    
    type MBrushStyle(__initial : Rabbyte.Drawing.BrushStyle) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Rabbyte.Drawing.BrushStyle> = Aardvark.Base.Incremental.EqModRef<Rabbyte.Drawing.BrushStyle>(__initial) :> Aardvark.Base.Incremental.IModRef<Rabbyte.Drawing.BrushStyle>
        let _primary = Aardvark.UI.Mutable.MColorInput.Create(__initial.primary)
        let _secondary = Aardvark.UI.Mutable.MColorInput.Create(__initial.secondary)
        let _lineStyle = MOption.Create(__initial.lineStyle)
        let _areaStyle = MOption.Create(__initial.areaStyle)
        let _thickness = ResetMod.Create(__initial.thickness)
        let _samplingRate = ResetMod.Create(__initial.samplingRate)
        
        member x.primary = _primary
        member x.secondary = _secondary
        member x.lineStyle = _lineStyle :> IMod<_>
        member x.areaStyle = _areaStyle :> IMod<_>
        member x.thickness = _thickness :> IMod<_>
        member x.samplingRate = _samplingRate :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Rabbyte.Drawing.BrushStyle) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                Aardvark.UI.Mutable.MColorInput.Update(_primary, v.primary)
                Aardvark.UI.Mutable.MColorInput.Update(_secondary, v.secondary)
                MOption.Update(_lineStyle, v.lineStyle)
                MOption.Update(_areaStyle, v.areaStyle)
                ResetMod.Update(_thickness,v.thickness)
                ResetMod.Update(_samplingRate,v.samplingRate)
                
        
        static member Create(__initial : Rabbyte.Drawing.BrushStyle) : MBrushStyle = MBrushStyle(__initial)
        static member Update(m : MBrushStyle, v : Rabbyte.Drawing.BrushStyle) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Rabbyte.Drawing.BrushStyle> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module BrushStyle =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let primary =
                { new Lens<Rabbyte.Drawing.BrushStyle, Aardvark.UI.ColorInput>() with
                    override x.Get(r) = r.primary
                    override x.Set(r,v) = { r with primary = v }
                    override x.Update(r,f) = { r with primary = f r.primary }
                }
            let secondary =
                { new Lens<Rabbyte.Drawing.BrushStyle, Aardvark.UI.ColorInput>() with
                    override x.Get(r) = r.secondary
                    override x.Set(r,v) = { r with secondary = v }
                    override x.Update(r,f) = { r with secondary = f r.secondary }
                }
            let lineStyle =
                { new Lens<Rabbyte.Drawing.BrushStyle, Microsoft.FSharp.Core.Option<Rabbyte.Drawing.LineStyle>>() with
                    override x.Get(r) = r.lineStyle
                    override x.Set(r,v) = { r with lineStyle = v }
                    override x.Update(r,f) = { r with lineStyle = f r.lineStyle }
                }
            let areaStyle =
                { new Lens<Rabbyte.Drawing.BrushStyle, Microsoft.FSharp.Core.Option<Rabbyte.Drawing.AreaStyle>>() with
                    override x.Get(r) = r.areaStyle
                    override x.Set(r,v) = { r with areaStyle = v }
                    override x.Update(r,f) = { r with areaStyle = f r.areaStyle }
                }
            let thickness =
                { new Lens<Rabbyte.Drawing.BrushStyle, System.Double>() with
                    override x.Get(r) = r.thickness
                    override x.Set(r,v) = { r with thickness = v }
                    override x.Update(r,f) = { r with thickness = f r.thickness }
                }
            let samplingRate =
                { new Lens<Rabbyte.Drawing.BrushStyle, System.Double>() with
                    override x.Get(r) = r.samplingRate
                    override x.Set(r,v) = { r with samplingRate = v }
                    override x.Update(r,f) = { r with samplingRate = f r.samplingRate }
                }
    
    
    type MDrawingModel(__initial : Rabbyte.Drawing.DrawingModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Rabbyte.Drawing.DrawingModel> = Aardvark.Base.Incremental.EqModRef<Rabbyte.Drawing.DrawingModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Rabbyte.Drawing.DrawingModel>
        let _points = MList.Create(__initial.points)
        let _segments = MList.Create(__initial.segments)
        let _style = MBrushStyle.Create(__initial.style)
        let _segmentCreation = ResetMod.Create(__initial.segmentCreation)
        let _past = ResetMod.Create(__initial.past)
        let _future = ResetMod.Create(__initial.future)
        let _primitiveType = ResetMod.Create(__initial.primitiveType)
        let _areaStyleNames = MMap.Create(__initial.areaStyleNames)
        let _lineStyleNames = MMap.Create(__initial.lineStyleNames)
        
        member x.points = _points :> alist<_>
        member x.segments = _segments :> alist<_>
        member x.style = _style
        member x.segmentCreation = _segmentCreation :> IMod<_>
        member x.past = _past :> IMod<_>
        member x.future = _future :> IMod<_>
        member x.primitiveType = _primitiveType :> IMod<_>
        member x.areaStyleNames = _areaStyleNames :> amap<_,_>
        member x.lineStyleNames = _lineStyleNames :> amap<_,_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Rabbyte.Drawing.DrawingModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MList.Update(_points, v.points)
                MList.Update(_segments, v.segments)
                MBrushStyle.Update(_style, v.style)
                ResetMod.Update(_segmentCreation,v.segmentCreation)
                _past.Update(v.past)
                _future.Update(v.future)
                ResetMod.Update(_primitiveType,v.primitiveType)
                MMap.Update(_areaStyleNames, v.areaStyleNames)
                MMap.Update(_lineStyleNames, v.lineStyleNames)
                
        
        static member Create(__initial : Rabbyte.Drawing.DrawingModel) : MDrawingModel = MDrawingModel(__initial)
        static member Update(m : MDrawingModel, v : Rabbyte.Drawing.DrawingModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Rabbyte.Drawing.DrawingModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module DrawingModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let points =
                { new Lens<Rabbyte.Drawing.DrawingModel, Aardvark.Base.plist<Aardvark.Base.V3d>>() with
                    override x.Get(r) = r.points
                    override x.Set(r,v) = { r with points = v }
                    override x.Update(r,f) = { r with points = f r.points }
                }
            let segments =
                { new Lens<Rabbyte.Drawing.DrawingModel, Aardvark.Base.plist<Rabbyte.Drawing.Segment>>() with
                    override x.Get(r) = r.segments
                    override x.Set(r,v) = { r with segments = v }
                    override x.Update(r,f) = { r with segments = f r.segments }
                }
            let style =
                { new Lens<Rabbyte.Drawing.DrawingModel, Rabbyte.Drawing.BrushStyle>() with
                    override x.Get(r) = r.style
                    override x.Set(r,v) = { r with style = v }
                    override x.Update(r,f) = { r with style = f r.style }
                }
            let segmentCreation =
                { new Lens<Rabbyte.Drawing.DrawingModel, Rabbyte.Drawing.SegmentCreation>() with
                    override x.Get(r) = r.segmentCreation
                    override x.Set(r,v) = { r with segmentCreation = v }
                    override x.Update(r,f) = { r with segmentCreation = f r.segmentCreation }
                }
            let past =
                { new Lens<Rabbyte.Drawing.DrawingModel, Microsoft.FSharp.Core.Option<Rabbyte.Drawing.DrawingModel>>() with
                    override x.Get(r) = r.past
                    override x.Set(r,v) = { r with past = v }
                    override x.Update(r,f) = { r with past = f r.past }
                }
            let future =
                { new Lens<Rabbyte.Drawing.DrawingModel, Microsoft.FSharp.Core.Option<Rabbyte.Drawing.DrawingModel>>() with
                    override x.Get(r) = r.future
                    override x.Set(r,v) = { r with future = v }
                    override x.Update(r,f) = { r with future = f r.future }
                }
            let primitiveType =
                { new Lens<Rabbyte.Drawing.DrawingModel, Rabbyte.Drawing.PrimitiveType>() with
                    override x.Get(r) = r.primitiveType
                    override x.Set(r,v) = { r with primitiveType = v }
                    override x.Update(r,f) = { r with primitiveType = f r.primitiveType }
                }
            let areaStyleNames =
                { new Lens<Rabbyte.Drawing.DrawingModel, Aardvark.Base.hmap<Rabbyte.Drawing.AreaStyle,System.String>>() with
                    override x.Get(r) = r.areaStyleNames
                    override x.Set(r,v) = { r with areaStyleNames = v }
                    override x.Update(r,f) = { r with areaStyleNames = f r.areaStyleNames }
                }
            let lineStyleNames =
                { new Lens<Rabbyte.Drawing.DrawingModel, Aardvark.Base.hmap<Rabbyte.Drawing.LineStyle,System.String>>() with
                    override x.Get(r) = r.lineStyleNames
                    override x.Set(r,v) = { r with lineStyleNames = v }
                    override x.Update(r,f) = { r with lineStyleNames = f r.lineStyleNames }
                }

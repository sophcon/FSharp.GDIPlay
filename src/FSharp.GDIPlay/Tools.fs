namespace FSharp.GDIPlay

open System
open System.Drawing
open System.Drawing.Imaging
open System.Runtime.InteropServices

module Tools =
    open System.Diagnostics

    type RawImageData = {
        data:byte[];
        stride:int
    }

    type RGBA = {
        red:int;
        green:int;
        blue:int;
        alpha:int
    }

    type RankedColor = {
        color:RGBA;
        rank:int;
    }

    type ColorDistance = {
        color1:RankedColor;
        color2:RankedColor;
        distance:float;
    }

    let calculateColorDistance c1 c2 =
        let redPortion   = Math.Pow((c1.red   |> float) - (c2.red   |> float), 2.0)
        let greenPortion = Math.Pow((c1.green |> float) - (c2.green |> float), 2.0)
        let bluePortion  = Math.Pow((c1.blue  |> float) - (c2.blue  |> float), 2.0)

        Math.Sqrt(redPortion + greenPortion + bluePortion)

    let calculateRankedColorDistance rc1 rc2 =
        calculateColorDistance rc1.color rc2.color

    let adjustLevel (channelByte:float32, level:float32) =
        channelByte * level

    let setInRGBRange channelByte =
        Math.Min(Math.Max(channelByte, 255.0f), 0.0f)

    let multiplyChannelByte (sourceChannel:byte) (overlayChannel:byte) =
        (float32(sourceChannel)/255.0f * float32(overlayChannel)/255.0f) * 255.0f |> byte

    let pixelMap2 (sourceImageData:byte[]) (overlayImageData:byte[]) (blendFunction) = 
        Array.map2 blendFunction sourceImageData overlayImageData

    let createSolidColorImage width height color  = 
        let image = new Bitmap(width, height, PixelFormat.Format32bppArgb)
        let graphics = Graphics.FromImage(image)

        graphics.FillRectangle(new SolidBrush(color), 0, 0, width, height)
        image

    let createSolidColorOverlay (image:Bitmap) (color:Color) = 
        createSolidColorImage image.Width image.Height color

    let newImageByteArray (imageData:BitmapData) =
        Array.zeroCreate<byte>(imageData.Stride * imageData.Height)

    let getByteArrayForImage (image:Bitmap) =
        let rect = Rectangle(0, 0, image.Width, image.Height)
        let data = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        let buffer = newImageByteArray data

        Marshal.Copy(data.Scan0, buffer, 0, buffer.Length)

        image.UnlockBits data

        { data = buffer; stride = data.Stride }
        
    let newBitmapFromImageData data =
        let height = data.data.Length / data.stride
        let width = data.stride / 4
        let bitmap = new Bitmap(width, height)
        let newImageData = bitmap.LockBits(Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb)

        Marshal.Copy(data.data, 0, newImageData.Scan0, data.data.Length) |> ignore

        bitmap.UnlockBits newImageData |> ignore

        bitmap

    let getImageData stride bytes =
        { data = bytes; stride = stride }

    let blendImages (baseImage:Bitmap) (overlayImage:Bitmap) =
        let baseData = getByteArrayForImage baseImage
        let overlayData = getByteArrayForImage overlayImage
    
        multiplyChannelByte
        |> pixelMap2 baseData.data overlayData.data
        |> getImageData baseData.stride
        |> newBitmapFromImageData
        
    let colorTransform image color =
        createSolidColorOverlay image color
        |> blendImages image

    let imageByteArrayToRGBArray (bytes:byte[]) =
        let bytesPerColor = 4
        let colorCount = bytes.Length / bytesPerColor
        if bytes.Length % bytesPerColor = 0 then
            [0..colorCount - 1]
            |> List.map (fun colorIndex ->
                {
                  blue  = bytes.[colorIndex * bytesPerColor + 0] |> int;
                  green = bytes.[colorIndex * bytesPerColor + 1] |> int;
                  red   = bytes.[colorIndex * bytesPerColor + 2] |> int;
                  alpha = bytes.[colorIndex * bytesPerColor + 3] |> int;})
            |> Some
        else
            None

    let getArgbFromRGBA color =
        Color.FromArgb(color.alpha, color.red, color.green, color.blue)

    let colorDistanceForListByReference refColor listOfColors =
        let refColorLocator = calculateColorDistance refColor

        let applyRefColorLocator listColor =
            (listColor, listColor |> refColorLocator)

        listOfColors
        |> List.map applyRefColorLocator

    let newBitmap (height:int) (width:int) =
        new Bitmap(width, height)

    let applyGraphics callback bitmap =
        use graphic = Graphics.FromImage(bitmap)
        graphic |> callback
        bitmap

    let generateRectangle sqHeight sqWidth maxPerColumn i color =
        let topOffset = (i % maxPerColumn) * sqHeight
        let leftOffset = (i / maxPerColumn) * sqWidth
        let argbColor = color |> getArgbFromRGBA

        (Rectangle(leftOffset, topOffset, sqHeight, sqWidth), argbColor)

    let generateLabels itemHeight itemWidth maxPerColumn i text =
        let topOffset = (i % maxPerColumn) * itemHeight |> float32
        let leftOffset = ((i / maxPerColumn) * itemWidth) + 25 |> float32

        (leftOffset, topOffset, text)


    let generateRectangles sqHeight sqWidth maxPerColumn colors =
        let rectGenerator = generateRectangle sqHeight sqWidth maxPerColumn

        colors |> List.mapi rectGenerator

    let rankColors colors =
        colors
        |> List.groupBy (fun color -> color)
        |> List.map (fun (color, group) -> { color = color; rank = group.Length})
        |> List.sortByDescending (fun rcolor -> rcolor.rank)

    let renderColorDistanceGraph colors =
        let sqHeight, sqWidth, maxPerCol = 25, 25, 100
        
        let rectangleGenerator = generateRectangle sqHeight sqWidth maxPerCol
        let labelGenerator = generateLabels sqHeight 300 100

        let labelData =
            colors
            |> List.map (fun dcolor -> dcolor.distance.ToString())
            |> List.mapi labelGenerator

        let rectangleData =
            colors
            |> List.map (fun dcolor -> dcolor.color2.color)
            |> List.mapi rectangleGenerator

        let height = sqHeight * maxPerCol + sqHeight
        let width = (((colors.Length / maxPerCol) + 1) * sqWidth) + 300

        newBitmap height width
        :> Image
        |> applyGraphics (fun g ->
            labelData
            |> List.map (fun (x, y, text) ->
                g.DrawString(text, new Font("Arial", float32(11)), new SolidBrush(Color.Black), x, y))
            |> ignore

            rectangleData
            |> List.map (fun (rect, color) -> 
                g.FillRectangle(new SolidBrush(color), rect))
            |> ignore)


    let graphImageColors top colors =
        let sqHeight, sqWidth, maxPerCol = 25, 25, 10

        let height = sqHeight * maxPerCol
        let width = ((top / maxPerCol) + 1) * sqWidth
        
        let rectangleGenerator = generateRectangle sqHeight sqWidth maxPerCol
          
        let rankedColors = rankColors colors

        newBitmap height width
        :> Image
        |> applyGraphics (fun g ->
            rankedColors
            |> List.map (fun rcolor -> rcolor.color)
            |> List.mapi rectangleGenerator
            |> List.map (fun (rect, color) -> 
                g.FillRectangle(new SolidBrush(color), rect))
            |> ignore)

    let createCompleteDistanceGraph rankedColors =
        
        let rec innerCreate colors =
            match colors with
            | head::tail ->

                let distances = 
                    tail
                    |> List.map (fun rcolor ->
                        let dist = rcolor |> calculateRankedColorDistance head
                        { color1 = head; color2 = rcolor; distance = dist })

                List.concat [distances; innerCreate tail]
            | [] -> []


        rankedColors |> innerCreate

    let createColorMap threshold distanceGraph =
        distanceGraph
        |> List.filter (fun dColor -> dColor.distance <= threshold)
        |> List.sortBy (fun dColor -> dColor.color1.rank)
        |> List.map (fun dColor -> dColor.color2.color, dColor.color1.color)
        |> Map.ofList

    let colorListToByteArray colors =
        colors
        |> List.collect (fun color -> 
            [
                (color.blue  |> byte);
                (color.green |> byte);
                (color.red   |> byte);
                (color.alpha |> byte);
            ])
        |> List.toArray

    let getMappedColorOrSelf (colorMap:Map<RGBA, RGBA>) color =
        match colorMap.TryFind(color) with
        | Some mappedColor -> mappedColor
        | None -> color

    let graphRankedColors height rankedColors =
        let columnWidth, padding = 25, 10

        let sum = 
            rankedColors
            |> List.sumBy (fun rankedColor -> rankedColor.rank |> float)

        let averages = 
            rankedColors 
            |> List.map (fun rankedColor -> 
                (rankedColor, ((rankedColor.rank |> float) / sum) ) )

        let max =
            averages |> List.map (fun (_, average) -> average) |> List.max

        let width = 
            (padding * (rankedColors.Length - 1)) + (columnWidth * rankedColors.Length - 1)

        let rectangleData =
            averages
            |> List.mapi (fun i (color, average) ->
                let leftOffset = i * (padding + columnWidth)
                let columnHeight = ((height |> float) * (average / max)) |> int
                let top = height - columnHeight

                (
                    new SolidBrush(color.color |> getArgbFromRGBA), 
                    Rectangle(leftOffset, top, columnWidth, columnHeight)
                ))

        newBitmap height width
        |> applyGraphics (fun g -> 
            rectangleData
            |> List.map (fun pair -> pair |> g.FillRectangle )
            |> ignore )

    let generateNoiseFromRankedColors width height rankedColors =
        let sum = 
            rankedColors
            |> List.sumBy (fun rankedColor -> rankedColor.rank |> float)

        let averages = 
            rankedColors 
            |> List.map (fun rankedColor -> 
                (rankedColor, ((rankedColor.rank |> float) / sum) ) )

        let pixelTotal = height * width
        let rnd = Random()

        newBitmap height width
        |> applyGraphics (fun g -> 
            let largest, _ = averages.[0]
            g.FillRectangle(new SolidBrush(largest.color |> getArgbFromRGBA), 0, 0, width, height)
            
            averages.[1..]
            |> List.map (fun (rankedColor, average) ->
                Debug.WriteLine(sprintf "Creating '%i' pixels" ((pixelTotal |> float) * average |> int))
                [0..((pixelTotal |> float) * average |> int)]
                |> List.map (fun _ -> 
                    let x, y =  rnd.Next(width), rnd.Next(height)
                    let color = new SolidBrush(rankedColor.color |> getArgbFromRGBA)
                    g.FillRectangle(color, x, y, 1, 1)
                ))
            |> ignore )
        
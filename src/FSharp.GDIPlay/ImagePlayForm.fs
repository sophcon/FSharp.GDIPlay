namespace FSharp.GDIPlay

open System
open System.Diagnostics
open System.Drawing
open System.Windows.Forms

module ImagePlayForm =
    let GetForm =
        let form = new Form()
        let imageControl = new PictureBox()
        let imageCloseColors = new PictureBox()
        let imageBefore = new PictureBox()
        let imageAfter = new PictureBox()
        let generate = new Button()
        let showColorDistribution = new Button()
        let rScroll = new HScrollBar()
        let gScroll = new HScrollBar()
        let bScroll = new HScrollBar()
        let distanceThreshold = new HScrollBar()
        let rLabel = new TextBox()
        let gLabel = new TextBox()
        let bLabel = new TextBox()

        rScroll.Top <- 0
        rScroll.Left <- 130
        rScroll.Maximum <- 255
        rScroll.Minimum <- 0
        rScroll.Width <- 300
        rScroll.Height <-15
        rScroll.ValueChanged.Add(fun a -> rLabel.Text <- rScroll.Value.ToString())

        rLabel.Top <- 0
        rLabel.Left <- 435
        rLabel.Width <- 50
        rLabel.Height <-15
        rLabel.Text <- "0"

        gScroll.Top <- 20
        gScroll.Left <- 130
        gScroll.Maximum <- 255
        gScroll.Minimum <- 0
        gScroll.Width <- 300
        gScroll.Height <-15
        gScroll.ValueChanged.Add(fun a -> gLabel.Text <- rScroll.Value.ToString())

        gLabel.Top <- 20
        gLabel.Left <- 435
        gLabel.Width <- 50
        gLabel.Height <-15
        gLabel.Text <- "0"

        bScroll.Top <- 40
        bScroll.Left <- 130
        bScroll.Maximum <- 255
        bScroll.Minimum <- 0
        bScroll.Width <- 300
        bScroll.Height <-15
        bScroll.ValueChanged.Add(fun a -> bLabel.Text <- rScroll.Value.ToString())

        bLabel.Top <- 40
        bLabel.Left <- 435
        bLabel.Width <- 50
        bLabel.Height <-15
        bLabel.Text <- "0"

        distanceThreshold.Top <- 65
        distanceThreshold.Left <- 130
        distanceThreshold.Maximum <- 400
        distanceThreshold.Minimum <- 0
        distanceThreshold.Width <- 300
        distanceThreshold.Height <- 15


        generate.Text <- "Generate"
        generate.Width <- 120
        generate.Height <- 24
        generate.Click.Add(fun a -> 
            match Utilities.GetFileFromDialog() with
            | Some path -> 
                let img = new Bitmap(path)
                let r = rLabel.Text |> int
                let g = gLabel.Text |> int
                let b = bLabel.Text |> int

                let color = Color.FromArgb(r, g, b)

                imageControl.Image <- Tools.colorTransform img color
            | None -> () )

        showColorDistribution.Text <- "Show Color Dist."
        showColorDistribution.Width <- 120
        showColorDistribution.Height <- 24
        showColorDistribution.Top <- 25
        showColorDistribution.Click.Add(fun a ->
            match Utilities.GetFileFromDialog() with
            | Some path ->
                let img = new Bitmap(path)
                let rawImgData = img |> Tools.getByteArrayForImage

                let colorList = 
                    match (rawImgData.data |> Tools.imageByteArrayToRGBArray) with
                    | Some colors -> colors
                    | None -> []

                let rankedColors = colorList |> Tools.rankColors

                let distanceGraph =
                    rankedColors |> Tools.createCompleteDistanceGraph


                imageControl.Image <- colorList |> Tools.graphImageColors 100
                imageCloseColors.Image <- 
                    distanceGraph
                    |> List.filter (fun dColor -> dColor.distance <= 0.0)
                    |> Tools.renderColorDistanceGraph

                imageControl.Click.Add (fun args ->
                    let mouseEvent = args :?> MouseEventArgs
                    let threshold = distanceThreshold.Value |> float

                    let coordString = sprintf "%i:%i" mouseEvent.X mouseEvent.Y
                    Debug.WriteLine(coordString)

                    let row = mouseEvent.Y / 25 |> int |> (+) 1
                    let column = mouseEvent.X / 25 |> int |> (+) 1
                    Debug.WriteLine("row: " + row.ToString() + ", col: " + column.ToString())
            
                    let index = (column - 1) * 10 + (row - 1)
                    Debug.WriteLine("index: " + index.ToString())

                    let refColor = rankedColors.[index]
                    let filteredGraph = 
                        distanceGraph
                        |> List.filter (fun dColor -> dColor.distance <= threshold)


                    imageCloseColors.Image <- 
                        filteredGraph
                        |> List.filter (fun dColor -> dColor.color1 = refColor)
                        |> Tools.renderColorDistanceGraph 

                    Debug.WriteLine("filtered graph: " + filteredGraph.Length.ToString())

                    let colorMap = distanceGraph |> Tools.createColorMap threshold
                    Debug.WriteLine("colorMap: " + colorMap.Count.ToString())
                        
                    imageBefore.Image <- img
                    imageBefore.Top <- 0

                    let mappedColors = 
                        colorList 
                        |> List.map (Tools.getMappedColorOrSelf colorMap)
                        |> Tools.colorListToByteArray

                    let newImage = 
                        {
                            Tools.RawImageData.stride = rawImgData.stride; 
                            Tools.RawImageData.data = mappedColors }
                        |> Tools.newBitmapFromImageData

                    imageAfter.Image <- newImage
                    imageAfter.Top <- img.Height + 10
                    )
                        
            | _ -> ()
        )

        imageControl.Top <- 95
        imageControl.AutoSize <- true
        imageCloseColors.Top <- 330
        imageCloseColors.AutoSize <- true
        imageBefore.Left <- 500
        imageBefore.AutoSize <- true
        imageAfter.Left <- 500
        imageAfter.AutoSize <- true
        imageAfter.Click.Add(fun _ ->
            let saveDialog = new SaveFileDialog()
            saveDialog.Filter <- "Jpeg File (*.jpg)|*.jpg"
            saveDialog.Title  <- "Save Processed Image"
            match saveDialog.ShowDialog() with
            | DialogResult.OK ->
                let saveFilePath = saveDialog.FileName
                imageAfter.Image.Save(saveFilePath, Imaging.ImageFormat.Jpeg)
            | _ -> () )
        form.Controls.Add(imageControl)
        form.Controls.Add(imageCloseColors)
        form.Controls.Add(imageBefore)
        form.Controls.Add(imageAfter)
        form.Controls.Add(generate)
        form.Controls.Add(showColorDistribution)
        form.Controls.Add(rScroll)
        form.Controls.Add(gScroll)
        form.Controls.Add(bScroll)
        form.Controls.Add(distanceThreshold)
        form.Controls.Add(rLabel)
        form.Controls.Add(gLabel)
        form.Controls.Add(bLabel)

        form
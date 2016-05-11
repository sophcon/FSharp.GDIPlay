﻿namespace FSharp.GDIPlay

module App =
    open System
    open System.Diagnostics
    open System.Drawing
    open System.Windows.Forms

    [<EntryPoint>]
    [<STAThread>]
    let main argv = 
        let form = new Form()
        let imageControl = new PictureBox()
        let imageCloseColors = new PictureBox()
        let generate = new Button()
        let showColorDistribution = new Button()
        let rScroll = new HScrollBar()
        let gScroll = new HScrollBar()
        let bScroll = new HScrollBar()
        let distanceThreshold = new HScrollBar()

        rScroll.Top <- 0
        rScroll.Left <- 130
        rScroll.Maximum <- 255
        rScroll.Minimum <- 0
        rScroll.Width <- 300
        rScroll.Height <-15

        gScroll.Top <- 20
        gScroll.Left <- 130
        gScroll.Maximum <- 255
        gScroll.Minimum <- 0
        gScroll.Width <- 300
        gScroll.Height <-15

        bScroll.Top <- 40
        bScroll.Left <- 130
        bScroll.Maximum <- 255
        bScroll.Minimum <- 0
        bScroll.Width <- 300
        bScroll.Height <-15

        distanceThreshold.Top <- 310
        distanceThreshold.Left <- 50
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
                let color = Color.FromArgb(rScroll.Value, gScroll.Value, bScroll.Value)

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
                    rawImgData.data
                    |> Tools.imageByteArrayToRGBArray

                match colorList with
                | Some c -> 
                    imageControl.Image <- c |> Tools.graphImageColors 100
                    imageCloseColors.Image <- c |> Tools.renderColorDistanceGraph 0 100 0.0
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

            
                        imageCloseColors.Image <- c |> Tools.renderColorDistanceGraph index 100 threshold)
                | _ -> ()
            | None -> ()
        )

        imageControl.Top <- 55
        imageControl.AutoSize <- true
        imageCloseColors.Top <- 330
        imageCloseColors.AutoSize <- true
        form.Controls.Add(imageControl)
        form.Controls.Add(imageCloseColors)
        form.Controls.Add(generate)
        form.Controls.Add(showColorDistribution)
        form.Controls.Add(rScroll)
        form.Controls.Add(gScroll)
        form.Controls.Add(bScroll)
        form.Controls.Add(distanceThreshold)

        form.ShowDialog() |> ignore
        0
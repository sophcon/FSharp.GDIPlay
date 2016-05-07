namespace FSharp.GDIPlay

module App =
    open System
    open System.Drawing
    open System.Windows.Forms
    open System.Windows.Input

    [<EntryPoint>]
    [<STAThread>]
    let main argv = 
        let form = new Form()
        let imageControl = new PictureBox()
        let generate = new Button()
        let rScroll = new HScrollBar()
        let gScroll = new HScrollBar()
        let bScroll = new HScrollBar()

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

        generate.Text <- "Generate"
        generate.Width <- 120
        generate.Height <- 24
        generate.Click.Add(fun a -> 
            form.Text <- "clicked!"
                
            match Utilities.GetFileFromDialog() with
            | Some path -> 
                let img = new Bitmap(path)

                imageControl.Image <- Tools.colorTransform img (Color.FromArgb(rScroll.Value, gScroll.Value, bScroll.Value))
            | None -> () )

        imageControl.Top <- 24
        imageControl.AutoSize <- true
        form.Controls.Add(imageControl)
        form.Controls.Add(generate)
        form.Controls.Add(rScroll)
        form.Controls.Add(gScroll)
        form.Controls.Add(bScroll)

        form.ShowDialog() |> ignore
        0
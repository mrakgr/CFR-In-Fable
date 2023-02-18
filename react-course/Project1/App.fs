module App
open Feliz

let body =
    Html.div [
        MyHtml.navbar
        Html.h1 "I'm learning React!"
        ]

ReactDOM.createRoot(Browser.Dom.document.getElementById "app").render(body)
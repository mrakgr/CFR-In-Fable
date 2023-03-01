module App

open Feliz

let root = Html.div [ My.Html.header; My.Html.main_content; My.Html.footer ]

ReactDOM.createRoot(Browser.Dom.document.getElementById "app").render(root)
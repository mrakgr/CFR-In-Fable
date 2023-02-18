module App

open Feliz

open Browser.Dom

ReactDOM.createRoot(document.getElementById "app")
    .render(Html.p "Hello, React!")
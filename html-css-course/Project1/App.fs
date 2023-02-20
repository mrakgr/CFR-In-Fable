module App

open Feliz

let button_subscribe =
    Html.button [
        prop.className "button-subscribe"
        prop.text "SUBSCRIBE"
        ]

let button_join =
    Html.button [
        prop.className "button-join"
        prop.text "JOIN"
        ]

let button_tweet =
    Html.button [
        prop.className "button-tweet"
        prop.text "Tweet"
        ]

let root =
    Html.div [
        Html.p "Hello HTML Course."
        Html.nav [
            button_subscribe
            button_join
            button_tweet
            ]
        Html.p [
            Html.a [
                prop.href "https://www.youtube.com"
                prop.target "_"
                prop.text "Link To Youtube"
                ]
            ]
        ]

ReactDOM.createRoot(Browser.Dom.document.getElementById "app").render(root)
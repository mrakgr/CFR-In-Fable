module App

open Feliz

let buttons =

    let button_subscribe =
        Html.button [
            prop.className "button-subscribe"
            prop.text "SUBSCRIBE"
            ]

    let button_join =
        Html.button [
            prop.className "button-join"
            prop.text "Join My Channel"
            ]

    let button_tweet =
        Html.button [
            prop.className "button-tweet"
            prop.text "Tweet"
            ]

    let root =
        Html.div [
            prop.className "button-paragraph"
            prop.children [
                Html.p "Hello HTML Course."
                Html.nav [
                    prop.className "button-nav"
                    prop.children [
                        button_subscribe
                        button_join
                        button_tweet
                        ]
                    ]
                Html.p [
                    Html.a [
                        prop.href "https://www.youtube.com"
                        prop.target "_"
                        prop.text "Link To Youtube"
                        ]
                    ]
                ]
            ]

    root

let root =
    Html.div [
        Html.p [
            prop.className "text-title"
            prop.text "Council conclusions on EU priorities in UN human rights fora 2023"
            ]
        Html.p [
            prop.className "text-stats"
            prop.text "3,413,157 views Feb 5, 2022 · 1 year ago"
            ]
        Html.p [
            prop.className "text-desc"
            prop.text "In this full course, we learn how to build websites with HTML and CSS, and get started as a software engineer.
Exercise solutions: https://supersimple.dev/courses/html-...
Copy of the code: https://supersimple.dev/courses/html-...
HTML and CSS reference: https://supersimple.dev/html"
            ]

        Html.p [
            prop.className "text-apple"
            prop.children [
                Html.text "Show early for the best seletion of holiday features. "
                Html.span [
                    prop.className "text-apple-link"
                    prop.href ""
                    prop.target "_blank"
                    prop.style [
                        style.marginLeft 15
                        ]
                    prop.text "Shop now >"
                    ]
                ]
            ]
        ]

ReactDOM.createRoot(Browser.Dom.document.getElementById "app").render(root)
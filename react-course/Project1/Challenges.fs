module Challenges

open Feliz

let navbar =
    Html.nav [
        Html.h1 "Rajnet"
        Html.ol [
            Html.li "Pricing"
            Html.li "About"
            Html.li "Contact"
            ]
        ]

let page =
    Html.div [
        Html.h1 "My Awesome Website"
        Html.h3 "Reasons I Love React"
        Html.ol [
            Html.li "qwe"
            ]
        ]

let challenge1 =
    Html.div [
        Html.img [
            prop.src "react-logo.svg"
            prop.width 100
            ]
        Html.h1 "Fun facts about React."
        Html.ul [
            Html.li "Was first released in 2013"
            Html.li "Was originally created by Jordan Walke"
            Html.li "Has well over 100k stars on Github"
            Html.li "Is maintained by Facebook"
            Html.li "Power thousands of enterprise apps, including mobile apps"
            ]
        ]

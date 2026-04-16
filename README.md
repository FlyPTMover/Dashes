# Mover TCP Stream Output Examples

This repository contains examples of how to use the **TCP stream output** in **Mover**.

## Overview

These examples demonstrate how telemetry data from Mover can be displayed in a browser-based dashboard. The goal is to provide a starting point for building custom dashboards that can be used both locally and remotely on other devices.

## Example 1 [open](https://flyptmover.github.io/Dashes/Example1.html)

![Cluster image](https://raw.githubusercontent.com/FlyPTMover/Dashes/main/Example1.jpg)

**Example 1** is a dashboard designed for car games, featuring:

- Gauges
- Extra driving and telemetry information
- Multiple pages that can be changed by sliding or tapping at the screen edges
- Browser-based access from other computers and mobile devices
- Full-screen mode with a double-click or double-tap

It is intended to work not only on the local PC, but also remotely from other computers, Android devices, and iOS devices through a browser.

This example was created entirely with **ChatGPT**, without traditional hand-written coding, to show how quickly a functional and customizable dashboard can be built with AI assistance.

## Creating Your Own Dashboard

One of the easiest ways to build your own dashboard is to start from one of the HTML examples in this repository and use it as a base.

A practical workflow is:

1. Take the HTML from one of the examples
2. Provide it to an AI tool
3. Describe what you want to build, for example: add a new page with a cluster based on an attached image
4. Ask for changes using terms such as **dash**, **gauges**, **cluster**, and **telemetry**

You should also use the **JSON** from Mover (the one you set in the TCP output) and ask the AI to use that JSON structure to build the interface around your available data.

## Recommendations

When creating your own dashboards, these guidelines may help:

- Use **SVG** elements whenever possible for scalable graphics
- Keep the layout responsive across different screen sizes
- Request compatibility with desktop and mobile browsers
- Design with support for different operating systems and devices in mind
- Structure the dashboard so pages and components are easy to expand and customize

import Chart from "./chart.js";

export function draw() {
    const links = [
        { source: "net6.0", target: "Azure.Storage.Blobs/12.10.0", type: "package" },
        { source: "net6.0", target: "Microsoft.Azure.Functions.Worker/1.6.0", type: "package" },
        { source: "net6.0", target: "Microsoft.PerflensBridge.StreamAnalysis/1.0.0", type: "project" },
        { source: "net6.0", target: "PerfLensBridge.CommonServices/1.0.0", type: "project" },
        { source: "Azure.Storage.Blobs/12.10.0", target: "Azure.Storage.Common/1.0.0", type: "package" },
        { source: "Azure.Storage.Blobs/12.10.0", target: "System.Text.Json/1.0.0", type: "package" },
    ];

    const types = ["package", "project"];
    const data = {
        nodes: Array.from(new Set(links.flatMap(l => [l.source, l.target])), id => ({ id })),
        links,
    };

    const chart = new Chart(data, types);
    const svg = chart.buildSvg();

    const svgHolder = document.getElementById('svgHolder');
    svgHolder.innerHTML = "";
    svgHolder.appendChild(svg);
}

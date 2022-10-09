export default class Chart {
    constructor(data, types, options = {
        //     x = (d, i) => i, // given d in data, returns the (ordinal) x-value
        //     y = d => d, // given d in data, returns the (quantitative) y-value
        //     title, // given d in data, returns the title text
        //     marginTop = 20, // the top margin, in pixels
        //     marginRight = 0, // the right margin, in pixels
        //     marginBottom = 30, // the bottom margin, in pixels
        //     marginLeft = 40, // the left margin, in pixels
        width: 880, // the outer width of the chart, in pixels
        height: 495,// the outer height of the chart, in pixels
        //     xDomain, // an array of (ordinal) x-values
        //     xRange = [marginLeft, width - marginRight], // [left, right]
        //     yType = d3.scaleLinear, // y-scale type
        //     yDomain, // [ymin, ymax]
        //     yRange = [height - marginBottom, marginTop], // [bottom, top]
        //     xPadding = 0.1, // amount of x-range to reserve to separate bars
        //     yFormat, // a format specifier string for the y-axis
        //     yLabel, // a label for the y-axis
        color: "currentColor" // bar fill color
    }) {
        this.data = data;
        this.types = types;
        this.options = options;
    }

    buildSvg() {
        const links = this.data.links.map(d => Object.create(d));
        const nodes = this.data.nodes.map(d => Object.create(d));

        const simulation = d3.forceSimulation(nodes)
            .force("link", d3.forceLink(links).id(d => d.id))
            .force("charge", d3.forceManyBody().strength(-400))
            .force("x", d3.forceX())
            .force("y", d3.forceY());

        const drag = simulation => {
            function dragstarted(event, d) {
                if (!event.active) simulation.alphaTarget(0.3).restart();
                d.fx = d.x;
                d.fy = d.y;
            }

            function dragged(event, d) {
                d.fx = event.x;
                d.fy = event.y;
            }

            function dragended(event, d) {
                if (!event.active) simulation.alphaTarget(0);
                d.fx = null;
                d.fy = null;
            }

            return d3.drag()
                .on("start", dragstarted)
                .on("drag", dragged)
                .on("end", dragended);
        }

        const width = this.options.width;
        const height = this.options.height;
        const color = this.options.color;

        const colorM = d3.scaleOrdinal(this.types, d3.schemeCategory10);

        const svg = d3.create("svg")
            .attr("viewBox", [-width / 2, -height / 2, width, height])
            .style("font", "6px sans-serif");

        // Per-type markers, as they don't inherit styles.
        svg.append("defs").selectAll("marker")
            .data(this.types)
            .join("marker")
            .attr("id", d => `arrow-${d}`)
            .attr("viewBox", "0 -5 10 10")
            .attr("refX", 15)
            .attr("refY", -0.5)
            .attr("markerWidth", 6)
            .attr("markerHeight", 6)
            .attr("orient", "auto")
            .append("path")
            .attr("fill", color)
            .attr("d", "M0,-5L10,0L0,5");

        const link = svg.append("g")
            .attr("fill", "none")
            .attr("stroke-width", 1.5)
            .selectAll("path")
            .data(links)
            .join("path")
            .attr("stroke", d => colorM(d.type))
            .attr("marker-end", d => `url(${new URL(`#arrow-${d.type}`, location)})`);

        const node = svg.append("g")
            .attr("fill", "currentColor")
            .attr("stroke-linecap", "round")
            .attr("stroke-linejoin", "round")
            .selectAll("g")
            .data(nodes)
            .join("g")
            .call(drag(simulation));

        node.append("circle")
            .attr("stroke", "white")
            .attr("stroke-width", 1.5)
            .attr("r", 3);  // Dot size

        node.append("text")
            .attr("x", 8)
            .attr("y", "0.31em")
            .text(d => d.id)
            .clone(true).lower()
            .attr("fill", "none")
            .attr("stroke", "white")
            .attr("stroke-width", 3);

        simulation.on("tick", () => {
            link.attr("d", linkArc);
            node.attr("transform", d => `translate(${d.x},${d.y})`);
        });

        function linkArc(d) {
            const r = Math.hypot(d.target.x - d.source.x, d.target.y - d.source.y);
            return `
                M${d.source.x},${d.source.y}
                A${r},${r} 0 0,1 ${d.target.x},${d.target.y}
            `;
        }

        // invalidation.then(() => simulation.stop());
        return svg.node();
    }
}
// @ts-check

//This file contains the functionality for the background perlin noise effect

var background = {};

background.backgroundRunning = false;

background.defaultTopColor = [107 / 2, 157 / 2, 62 / 2, 255];
background.defaultBottomColor = [50 / 4, 90 / 4, 30 / 4, 255];

background.currentTopColor = background.defaultTopColor.slice();
background.currentBottomColor = background.defaultBottomColor.slice();

background.darkenAmount = 0.75;
background.scale = 750.0;
background.speed = 0.15;
background.parallaxAmount = 3.0;

background.qualty = 1;

background.uniforms = {};

/** @type {HTMLCanvasElement} */
background.glCanvas = null;

/** @type {WebGLRenderingContext} */
background.gl = null;


background.interpolationTimer = 0;

/** @type {Array.<number>} */
background.startTopColor = background.defaultTopColor.slice();

/** @type {Array.<number>} */
background.endTopColor = background.defaultTopColor.slice();

/** @type {Array.<number>} */
background.startBottomColor = background.defaultBottomColor.slice();

/** @type {Array.<number>} */
background.endBottomColor = background.defaultBottomColor.slice();

background.timer = 0.0;

background.vertexShader = "";
background.fragmentShader = "";

background.previousTime = 0;

var onMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);


/**
 * 
 * @param {number} c
 * @return {string}
 */
function componentToHex(c) {
	var hex = Math.round(c).toString(16);
	return hex.length == 1 ? "0" + hex : hex;
}

/**
 * 
 * @param {Array.<number>} color
 * @returns {string}
 */
background.rgbToHex = function (color) {
	var result = "#" + componentToHex(color[0]) + componentToHex(color[1]) + componentToHex(color[2]);
	if (color.length >= 4) {
		result += componentToHex(color[3]);
	}
	return result;
}

/**
 * 
 * @param {string} hex
 * @returns {Array.<number>}
 */
background.hexToRgb = function (hex) {
	hex = hex.replace(" ", "");
	var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
	if (result) {
		return [parseInt(result[1], 16), parseInt(result[2], 16), parseInt(result[3], 16), parseInt(result[4], 16)];
	}
	else {
		result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
		if (result) {
			return [parseInt(result[1], 16), parseInt(result[2], 16), parseInt(result[3], 16), 255];
		}
		return null;
	}
}

/**
 * 
 * @param {string} value
 * @returns {Array.<number>}
 */
background.cssToColor = function (value) {
	if (value.startsWith("#")) {
		return background.hexToRgb(value);
	}
	if (value.startsWith("rgba")) {
		value = value.slice(5, -1);
	}
	else {
		value = value.slice(4, -1);
	}

	var result = value.split(", ");

	var colorResult = [0, 0, 0, 1.0];


	for (var i = 0; i < result.length; i++) {
		colorResult[i] = Number(result[i]);
	}

	colorResult[3] *= 255;

	return colorResult;
}

/**
 * 
 * @param {Array.<number>} colors
 * @returns {string}
 */
background.colorToCSS = function (colors) {
	if (colors.length > 3) {
		return "rgba(" + colors[0] + ", " + colors[1] + ", " + colors[2] + ", " + (colors[3] / 255.0) + ")";
	}
	else {
		return "rgb(" + colors[0] + ", " + colors[1] + ", " + colors[2] + ")";
	}
}


/*var background.backgroundRunning = false;
var background.Scale = 750.0;
var background.speed = 0.15;
var background.parallaxAmount = 3.0;
var resolutionMultipler = 0.5;
var background.darkenAmount = 0.75;

var background.currentTopColor = null;
var background.currentBottomColor = null;

var defaultTopColor = [107 / 2, 157 / 2, 62 / 2, 255];
var defaultBottomColor = [50 / 4, 90 / 4, 30 / 4, 255];
//var defaultBottomColor = [34,40,42,255];

var interpolateTimeMax = null;
var interpolateTime = null;
var interpolateTopColor = null;
var interpolateBottomColor = null;

var interpolatePreviousTopColor = null;
var interpolatePreviousBottomColor = null;*/

/*function DarkenColor(color, amount) {
	return lerpColor(color, [0, 0, 0, 255], amount);
}*/

function onWindowLoad() {
	background.glCanvas = document.getElementById('background-canvas');
	background.gl = background.glCanvas.getContext("webgl");
	if (background.gl === null) {
		return;
	}
	Promise.all([fetch("js/shaders/perlin.vert"), fetch("js/shaders/perlin.frag")]).then(responses => Promise.all([responses[0].text(), responses[1].text()])).then(responses => {
		background.vertexShader = responses[0];
		background.fragmentShader = responses[1];

		background.backgroundRunning = true;

		if (onMobile) {
			background.qualty /= 2;
		}

		setupGlContext();

		background.glCanvas.style.opacity = "1";
	});


	window.requestAnimationFrame(loop);
}

/*
background.interpolateToColor = function (topColor, bottomColor) {

	if (onMobile) {
		background.currentTopColor = background.cssToColor(topColor);
		background.currentBottomColor = background.cssToColor(bottomColor);
		return;
	}

	//core.removeFromEvent(core.events.updateEvent, background.bg_revert_colors_update);
	//core.removeFromEvent(core.events.updateEvent, background.bg_interpolate_colors_update);

	background.startTopColor = core.lerpArray(background.startTopColor, background.endTopColor, background.interpolationTimer);
	background.startBottomColor = core.lerpArray(background.startBottomColor, background.endBottomColor, background.interpolationTimer);

	background.interpolationTimer = 0.0;

	background.endTopColor = core.cssToColor(topColor);
	background.endBottomColor = core.cssToColor(bottomColor);

	//core.addToEvent(core.events.updateEvent, background.bg_interpolate_colors_update);
}*/

/*background.revertInterpolation = function () {

	if (core.onMobile) {
		background.currentTopColor = background.defaultTopColor.slice();
		background.currentBottomColor = background.defaultBottomColor.slice();
		return;
	}

	core.removeFromEvent(core.events.updateEvent, background.bg_interpolate_colors_update);
	core.removeFromEvent(core.events.updateEvent, background.bg_revert_colors_update);

	background.endTopColor = core.lerpArray(background.startTopColor, background.endTopColor, background.interpolationTimer);
	background.endBottomColor = core.lerpArray(background.startBottomColor, background.endBottomColor, background.interpolationTimer);

	background.startTopColor = background.defaultTopColor.slice();
	background.startBottomColor = background.defaultBottomColor.slice();

	background.interpolationTimer = 1.0;

	core.addToEvent(core.events.updateEvent, background.bg_revert_colors_update);
}*/

/*background.bg_interpolate_colors_update = function (dt) {
	background.interpolationTimer += dt * projectPanel.interpolationSpeed;
	if (background.interpolationTimer >= 1) {
		background.interpolationTimer = 1;
		core.removeFromEvent(core.events.updateEvent, background.interpolation_color_update);
	}

	calculate_colors();
}

background.bg_revert_colors_update = function (dt) {
	background.interpolationTimer -= dt * projectPanel.interpolationSpeed;
	if (background.interpolationTimer <= 0) {
		background.interpolationTimer = 0;
		core.removeFromEvent(core.events.updateEvent, background.revert_color_update);
	}

	calculate_colors();
}*/

/*function calculate_colors() {
	background.currentTopColor = core.lerpArray(background.startTopColor, background.endTopColor, background.interpolationTimer);
	background.currentBottomColor = core.lerpArray(background.startBottomColor, background.endBottomColor, background.interpolationTimer);
}*/

function update(dt) {
	background.timer += dt;
	if (!background.backgroundRunning) {
		return;
	}

	if (!(onMobile && projectPanel.projectOpen && window.innerWidth < (59.375 * 16))) {
		drawBackground();
	}



	/*if (background.backgroundRunning === false) {
		return;
	}

	var root = document.documentElement;

	if (interpolateTime !== null) {
		background.currentTopColor = lerpColor(interpolatePreviousTopColor, interpolateTopColor, 1.0 - (interpolateTime / interpolateTimeMax));
		background.currentBottomColor = lerpColor(interpolatePreviousBottomColor, interpolateBottomColor, 1.0 - (interpolateTime / interpolateTimeMax));


		interpolateTime -= dt;
		//console.log("Interpolate Time = " + interpolateTime);
		if (interpolateTime <= 0) {
			background.currentTopColor = lerpColor(interpolatePreviousTopColor, interpolateTopColor, 1.0);
			background.currentBottomColor = lerpColor(interpolatePreviousBottomColor, interpolateBottomColor, 1.0);
			interpolateTime = null;
		}

		//console.log("CURRENT TOP COLOR = " + background.currentTopColor);
		//console.log("NEW TOP COLOR = " + rgbToHex(background.currentTopColor));

		//root.style.setProperty('--project-window-top-color', rgbToHex(background.currentTopColor));
		//root.style.setProperty('--project-window-bottom-color', rgbToHex(background.currentBottomColor));
	}
	if (!(onMobile && projectOpen && window.innerWidth < 950)) {
		drawBackground();
	}*/
}

/*function InterpolateToNewColor(newTopColor, newBottomColor, time) {
	interpolateTime = time;
	interpolateTimeMax = time;
	if (background.currentTopColor === undefined || background.currentTopColor === null) {
		background.currentTopColor = defaultTopColor;
		background.currentBottomColor = defaultBottomColor;
	}
	interpolatePreviousTopColor = background.currentTopColor.slice();
	interpolatePreviousBottomColor = background.currentBottomColor.slice();

	interpolateTopColor = newTopColor.slice();
	interpolateBottomColor = newBottomColor.slice();
}

function lerp(a, b, t) {
	return (1 - t) * a + t * b;
}

function lerpColor(colorA, colorB, t) {
	if (colorA.length >= 4 || colorB.length >= 4) {
		var alpha = lerp(colorA.length >= 4 ? colorA[3] : 255, colorB.length >= 4 ? colorB[3] : 255, t);
		return [lerp(colorA[0], colorB[0], t), lerp(colorA[1], colorB[1], t), lerp(colorA[2], colorB[2], t), alpha];
	}
	else {
		return [lerp(colorA[0], colorB[0], t), lerp(colorA[1], colorB[1], t), lerp(colorA[2], colorB[2], t)];
	}
}*/

function drawBackground() {
	//var documentStyle = getComputedStyle(document.documentElement);



	/*background.glCanvas.width = window.innerWidth;
	background.glCanvas.height = window.innerHeight;

	background.glCanvas.width = Math.max(window.innerWidth,document.body.scrollWidth);
	background.glCanvas.height = Math.max(window.innerHeight,document.body.scrollHeight);*/


	//var width = window.innerWidth;
	//var height = window.innerHeight;

	//var width = Math.max(document.body.clientWidth, window.innerWidth);
	//var height = Math.max(document.body.clientHeight, window.innerHeight);

	//var width2 = Math.max(document.body.clientWidth, window.innerWidth);
	//var width = background.glCanvas.width;
	var width = document.body.clientWidth;
	var height = document.getElementById("informational-section").scrollHeight;
	//var height = background.glCanvas.height;

	//console.log("Width = " + width);
	//console.log("Height = " + height);



	background.glCanvas.width = width * background.qualty;
	background.glCanvas.height = height * background.qualty;

	background.glCanvas.style.width = width.toString() + "px";
	background.glCanvas.style.height = height.toString() + "px";

	//var width = document.body.clientWidth;
	//var height = document.body.clientHeight;

	//background.glCanvas.style.width = width.toString() + "px";
	//background.glCanvas.style.height = height.toString() + "px";



	//background.glCanvas.width = width * background.qualty;
	//background.glCanvas.height = height * background.qualty;

	/*background.glCanvas.width = width * background.qualty;
	background.glCanvas.height = height * background.qualty;

	background.glCanvas.style.width = width.toString() + "px";
	background.glCanvas.style.height = height.toString() + "px";*/




	//background.glCanvas.width = "100%";

	//background.glCanvas.width = window.innerWidth;//Math.max(window.innerWidth,document.body.scrollWidth);
	//background.glCanvas.height = window.innerHeight;//Math.max(window.innerHeight,document.body.scrollHeight);

	//background.glCanvas.width = Math.max(window.innerWidth,window.scrollWidth);
	//background.glCanvas.height = Math.max(window.innerHeight,window.scrollHeight);
	//Set background.uniforms
	background.uniforms.setNoiseScale(background.scale * background.qualty);
	background.uniforms.setTopColor(background.currentTopColor);
	background.uniforms.setBottomColor(background.currentBottomColor);

	background.uniforms.setNoiseZ((background.timer) * background.speed);
	background.uniforms.setVerticalOffset((window.scrollY / background.parallaxAmount) - (window.scrollY * background.qualty) - height);
	// Clear the canvas
	background.gl.clearColor(1.0, 0.0, 0.0, 1.0);

	// Clear the color buffer bit
	background.gl.clear(background.gl.COLOR_BUFFER_BIT);
	// Set the view port
	//console.log("Width = " + window.innerWidth + " Height = " + window.innerHeight);
	background.gl.viewport(0, 0, width * background.qualty, height * background.qualty);

	// Draw the triangle
	//background.gl.drawArrays(background.gl.POINTS, 0, 6);
	background.gl.drawElements(background.gl.TRIANGLES, 6, background.gl.UNSIGNED_SHORT, 0);
	//console.log("DRAWING");

	//ErrorCheck();

	//requestAnimationFrame(drawBackground);
}

/*function GetTrueHeight() {
	var body = document.body, html = document.documentElement;

	var height = Math.max(body.scrollHeight, body.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight);
	return height;
}*/

function loop(time) {
	update((time - background.previousTime) / 1000.0);

	background.previousTime = time;
	window.requestAnimationFrame(loop);
}

//var previousTime = 0;

function setupGlContext() {

	//console.log("BEGIN ERROR = " + background.gl.getError());
	//testPrint("Setting up Context");
	//Define the vertex points
	var verticies = [
		-1.0, -1.0, 0.0,
		-1.0, 1.0, 0.0,
		1.0, 1.0, 0.0,
		1.0, -1.0, 0.0,
	];

	var indices = [0, 1, 3, 1, 2, 3];

	//Create the vertex buffer
	var vertex_buffer = background.gl.createBuffer();
	background.gl.bindBuffer(background.gl.ARRAY_BUFFER, vertex_buffer);
	background.gl.bufferData(background.gl.ARRAY_BUFFER, new Float32Array(verticies), background.gl.STATIC_DRAW);
	background.gl.bindBuffer(background.gl.ARRAY_BUFFER, null);

	//Create the index buffer
	var index_buffer = background.gl.createBuffer();
	background.gl.bindBuffer(background.gl.ELEMENT_ARRAY_BUFFER, index_buffer);
	background.gl.bufferData(background.gl.ELEMENT_ARRAY_BUFFER, new Uint16Array(indices), background.gl.STATIC_DRAW);
	background.gl.bindBuffer(background.gl.ELEMENT_ARRAY_BUFFER, null);

	//Create Vertex Shader
	//var vertCode = window.shaders.backgroundVertexShader;
	var vertShader = background.gl.createShader(background.gl.VERTEX_SHADER);
	background.gl.shaderSource(vertShader, background.vertexShader);
	background.gl.compileShader(vertShader);
	var message = background.gl.getShaderInfoLog(vertShader);
	if (message.length > 0) {
		console.log("Vertex Shader Compilation Error");
		console.log(message);
	}

	//Create Fragment/Pixel Shader
	//var fragCode = window.shaders.backgroundFragmentShader;

	var fragShader = background.gl.createShader(background.gl.FRAGMENT_SHADER);
	background.gl.shaderSource(fragShader, background.fragmentShader);
	background.gl.compileShader(fragShader);

	var message = background.gl.getShaderInfoLog(fragShader);
	if (message.length > 0) {
		console.log("Fragment Shader Compilation Error");
		console.log(message);
	}

	//Create and use Shader Program
	var shaderProgram = background.gl.createProgram();
	background.gl.attachShader(shaderProgram, vertShader);
	background.gl.attachShader(shaderProgram, fragShader);
	background.gl.linkProgram(shaderProgram);
	background.gl.useProgram(shaderProgram);

	//Get background.uniforms
	background.uniforms.noiseScale = background.gl.getUniformLocation(shaderProgram, "noiseScale");
	background.uniforms.setNoiseScale = function (value) {
		background.gl.uniform1f(background.uniforms.noiseScale, value);
	};

	background.uniforms.noiseZ = background.gl.getUniformLocation(shaderProgram, "noiseZ");
	background.uniforms.setNoiseZ = function (value) {
		background.gl.uniform1f(background.uniforms.noiseZ, value);
	};

	background.uniforms.verticalOffset = background.gl.getUniformLocation(shaderProgram, "verticalOffset");
	background.uniforms.setVerticalOffset = function (value) {
		background.gl.uniform1f(background.uniforms.verticalOffset, value);
	};

	background.uniforms.topColor = background.gl.getUniformLocation(shaderProgram, "topColor");
	background.uniforms.setTopColor = function (value) {
		background.gl.uniform4f(background.uniforms.topColor, (value[0] / 255.0) * background.darkenAmount, (value[1] / 255.0) * background.darkenAmount, (value[2] / 255.0) * background.darkenAmount, value[3] / 255.0);
	};

	background.uniforms.bottomColor = background.gl.getUniformLocation(shaderProgram, "bottomColor");
	background.uniforms.setBottomColor = function (value) {
		background.gl.uniform4f(background.uniforms.bottomColor, (value[0] / 255.0) * background.darkenAmount, (value[1] / 255.0) * background.darkenAmount, (value[2] / 255.0) * background.darkenAmount, value[3] / 255.0);
	};

	//Bind Buffers to Shader Program
	background.gl.bindBuffer(background.gl.ARRAY_BUFFER, vertex_buffer);
	background.gl.bindBuffer(background.gl.ELEMENT_ARRAY_BUFFER, index_buffer);
	var coord = background.gl.getAttribLocation(shaderProgram, "position");
	background.gl.vertexAttribPointer(coord, 3, background.gl.FLOAT, false, 0, 0);
	background.gl.enableVertexAttribArray(coord);
	// Disable the depth test
	background.gl.disable(background.gl.DEPTH_TEST);

	//console.log("END ERROR = " + background.gl.getError());
	//ErrorCheck();
}

//window.onload = onWindowLoad;

//window.onload = onWindowLoad;
//core.addToEvent(core.events.onWindowLoad, onWindowLoad);

//core.addToEvent(core.events.updateEvent, update);

/*core.addToEvent(projectPanel.events.projectColorChangeEvent, project => {
	//console.log("Color Chance = " + project);
	if (project == null) {
		background.revertInterpolation();
	}
	else {
		var color = project.color;
		if (project.perlinColor) {
			color = project.perlinColor;
		}

		var backColor = project.backgroundColor;
		if (project.perlinBackgroundColor) {
			backColor = project.perlinBackgroundColor;
		}

		background.interpolateToColor(color, backColor);
	}
});*/

//ResetColors();

/*function ErrorCheck()
{
	var error = background.gl.getError();
	if (error !== background.gl.NO_ERROR) {
		console.log(error);
	}
}*/

/*function testPrint(message)
{
	var debugTest = document.getElementById('debugTest');
	debugTest.innerText += message + "-:-";
}*/
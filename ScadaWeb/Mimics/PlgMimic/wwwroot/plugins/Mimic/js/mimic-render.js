﻿// Contains classes: Renderer, MimicRenderer, ComponentRenderer,
// TextRenderer, PictureRenderer, PanelRenderer, RenderContext, RendererSet
// Depends on jquery, mimic-common.js

// Represents a renderer of a mimic or component.
rs.mimic.Renderer = class {
    // Sets the left and top of the specified jQuery object.
    _setLocation(jqObj, location) {
        jqObj.css({
            "left": location.x + "px",
            "top": location.y + "px"
        });
    }

    // Sets the width and height of the specified jQuery object.
    _setSize(jqObj, size) {
        jqObj.css({
            "width": size.width + "px",
            "height": size.height + "px"
        });
    }

    // Creates a DOM content of the component according to the model. Returns a jQuery object.
    createDom(component, renderContext) {
        return null;
    }

    // Updates the component according to the current channel data.
    update(component, renderContext) {
    }
}

// Represents a mimic renderer.
rs.mimic.MimicRenderer = class extends rs.mimic.Renderer {
    createDom(component, renderContext) {
        component.dom = $("<div class='mimic'></div>");
        this.updateDom(component, renderContext);
        return component.dom;
    }

    updateDom(component, renderContext) {
        if (component.dom instanceof jQuery) {
            let mimicElem = component.dom.first();
            this._setSize(mimicElem, component.document.size);
        }
    }
}

// Represents a component renderer.
rs.mimic.ComponentRenderer = class extends rs.mimic.Renderer {
    createDom(component, renderContext) {
        component.dom = $("<div id='comp" + component.id + "' class='comp'></div>");
        return component.dom;
    }
}

// Represents a text component renderer.
rs.mimic.TextRenderer = class extends rs.mimic.ComponentRenderer {
    createDom(component, renderContext) {
        let textElem = super.createDom(component, renderContext);
        let props = component.properties;
        textElem.addClass("text").text(props.text);
        this._setLocation(textElem, props.location);
        this._setSize(textElem, props.size);
        return textElem;
    }
}

// Represents a picture component renderer.
rs.mimic.PictureRenderer = class extends rs.mimic.ComponentRenderer {
    createDom(component, renderContext) {
        let pictureElem = super.createDom(component, renderContext);
        let props = component.properties;
        pictureElem.addClass("picture");
        this._setLocation(pictureElem, props.location);
        this._setSize(pictureElem, props.size);
        return pictureElem;
    }
}

// Represents a panel component renderer.
rs.mimic.PanelRenderer = class extends rs.mimic.ComponentRenderer {
    createDom(component, renderContext) {
        let panelElem = super.createDom(component, renderContext);
        panelElem.addClass("panel");
        this.updateDom(component, renderContext);
        return panelElem;
    }

    updateDom(component, renderContext) {
        if (component.dom instanceof jQuery) {
            let panelElem = component.dom.first();
            let props = component.properties;
            this._setLocation(panelElem, props.location);
            this._setSize(panelElem, props.size);
        }
    }
}

// Encapsulates information about a rendering operation.
rs.mimic.RenderContext = class {
    editMode = false;
}

// Contains renderers for a mimic and its components.
rs.mimic.RendererSet = class {
    mimicRenderer = new rs.mimic.MimicRenderer();
    componentRenderers = new Map([
        ["Text", new rs.mimic.TextRenderer()],
        ["Picture", new rs.mimic.PictureRenderer()],
        ["Panel", new rs.mimic.PanelRenderer()]
    ]);
}

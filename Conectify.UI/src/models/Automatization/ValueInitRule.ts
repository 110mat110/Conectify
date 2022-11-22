import { AutomatizationBaseWithTarget } from "../automatizationComponent";

export class ValueInitRule extends AutomatizationBaseWithTarget {

    public sourceId: string = "";

    public getParametersJSon(): string {
        return "{\"SourceSensorId\" : \"" + this.sourceId + "\"}";
    };

    constructor(id: string, behaviourId: string, json: string) {
        super(id, behaviourId)
        try {
            const obj = JSON.parse(json);
            if (obj.sourceSensorId) {
                this.sourceId = obj.sourceSensorId;
            }
        }
        catch (e) {
            console.error(e);
        }
    }
}
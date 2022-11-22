import { AutomatizationBaseWithTarget } from "../automatizationComponent";

export class UserInputRule extends AutomatizationBaseWithTarget {

    destinationId: string = "";

    public getParametersJSon(): string {
        return "{\"SourceSensorId\" : \"" + this.destinationId + "\"}";
    };

    constructor(id: string, behaviourId: string, json: string) {
        super(id, behaviourId)
        try {
            const obj = JSON.parse(json);
            if (obj.sourceSensorId) {
                this.destinationId = obj.sourceSensorId;
            }
        }
        catch (e) {
            console.error(e);
        }
    }
}
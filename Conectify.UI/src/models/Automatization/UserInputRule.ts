import { AutomatizationBaseWithTarget } from "../automatizationComponent";

export class UserInputRule extends AutomatizationBaseWithTarget {

    destinationId: string = "";
    name: string = "unknown";

    public getParametersJSon(): string {
        return "{\"SourceActuatorId\" : \"" + this.destinationId + "\"}";
    };

    constructor(id: string, behaviourId: string, json: string) {
        super(id, behaviourId)
        try {
            const obj = JSON.parse(json);
            if (obj) {
                this.destinationId = obj.SourceActuatorId;
                this.name = obj.Name;
            }
        }
        catch (e) {
            console.error(e);
        }
    }
}
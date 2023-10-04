import { AutomatizationBaseWithTarget } from "../automatizationComponent";

export class ChangeDestinationRule extends AutomatizationBaseWithTarget {

    destinationId: string = "";
    name: string = "unknown";

    public getParametersJSon(): string {
        return "{\"DestinationId\" : \"" + this.destinationId + "\"}";
    };

    constructor(id: string, behaviourId: string, json: string) {
        super(id, behaviourId)
        try {
            let obj = JSON.parse(json);
            if (obj) {
                this.destinationId = obj.DestinationId;
                this.name = obj.Name;
            }
        }
        catch (e) {
            console.error(e);
        }
    }
}
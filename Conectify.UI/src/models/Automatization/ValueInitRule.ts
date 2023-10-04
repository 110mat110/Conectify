import { AutomatizationBaseWithTarget } from "../automatizationComponent";

export class ValueInitRule extends AutomatizationBaseWithTarget {

    public sourceId: string = "";
    public name:string = "unknown";

    public getParametersJSon(): string {
        return "{\"SourceSensorId\" : \"" + this.sourceId + "\"}";
    };

    constructor(id: string, behaviourId: string, json: string) {
        super(id, behaviourId)
        try {
            const obj = JSON.parse(json);
            if (obj) {
                this.sourceId = obj.SourceSensorId;
                this.name = obj.Name;
            }
        }
        catch (e) {
            console.error(e);
        }
    }
}
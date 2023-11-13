export class AutomatizationBase{
    public id: string;

    public dragPosition = {x: 0, y: 0};

    public drawingPos = {x: 0, y: 0};
    public behaviorId: string = "";

    public targets: string[] = [];
    public parameters: string[] = [];

    public getParametersJSon(): string {
        return "{}";
    };

    constructor(id: string, behaviourId: string){
        this.id = id;
        this.behaviorId = behaviourId;
    }
}

export class AutomatizationBaseGeneric<T> extends AutomatizationBase{
    public behaviour: T;

    public getParametersJSon(): string {
        return JSON.stringify(this.behaviour);
    };

    constructor(id: string, behaviourId: string, json: string, defaultT: T){
    super(id, behaviourId)
        this.behaviour = defaultT;
        try {
            let behaviour: T = JSON.parse(json) as T;
            if (behaviour) {
                this.behaviour = behaviour;
            }
        }
        catch (e) {
            console.error(e);
        }
    }
}
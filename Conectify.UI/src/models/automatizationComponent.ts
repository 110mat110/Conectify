export class AutomatizationBase{
    public id: string;

    public dragPosition = {x: 0, y: 0};

    public drawingPos = {x: 0, y: 0};
    public behaviorId: string = "";

    public getParametersJSon(): string{
        return "{}";
    }

    constructor(id: string, behaviourId: string){
        this.id = id;
        this.behaviorId = behaviourId;
    }
}


export class AutomatizationBaseWithTarget extends AutomatizationBase{

    public targets: string[] = [];

    constructor(id: string, behaviourId: string){
        super(id, behaviourId);
    }
}
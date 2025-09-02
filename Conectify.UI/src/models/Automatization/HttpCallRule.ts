import { AutomatizationBaseGeneric } from "../automatizationComponent";

export class HttpCallRule extends AutomatizationBaseGeneric<HttpCallRuleBehaviour> {
}

interface HttpCallRuleBehaviour{
    Http : string;
}
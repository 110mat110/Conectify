import { AutomatizationBaseGeneric } from "../automatizationComponent";

/** Options of a port of a function. The name is what the pin on the parent board is labelled with. */
export interface PortRuleOptions {
    Name: string;
}

export class PortRule extends AutomatizationBaseGeneric<PortRuleOptions> {
}

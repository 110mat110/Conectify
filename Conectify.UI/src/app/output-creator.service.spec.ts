import { TestBed } from '@angular/core/testing';

import { OutputCreatorService } from './output-creator.service';

describe('OutputCreatorService', () => {
  let service: OutputCreatorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(OutputCreatorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

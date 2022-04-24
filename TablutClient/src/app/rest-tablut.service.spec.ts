import { TestBed } from '@angular/core/testing';

import { RestTablutService } from './rest-tablut.service';

describe('RestTablutService', () => {
  let service: RestTablutService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RestTablutService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

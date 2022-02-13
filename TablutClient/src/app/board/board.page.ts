import { Component, OnInit } from '@angular/core';
import { Grid } from '../models/grid';
import { BLACK_PIECE, WHITE_KING, WHITE_PIECE } from '../models/pieces';
import { PieceInterface } from '../models/pieces-interface';

@Component({
  selector: 'app-board',
  templateUrl: './board.page.html',
  styleUrls: ['./board.page.scss'],
})
export class BoardPage implements OnInit {

  turn = 'white';
  row_i = -1;
  col_i = -1;
  selectedPiece: PieceInterface | null;
  steps = [];
  showbrd = false;
  tablutGrid: any;
  readonly SIZE = 9;
  readonly MIDDLE = Math.floor(this.SIZE/2);

  constructor() { }

  ngOnInit() {
    this.initBoard();
  }

  async initBoard(){
    this.tablutGrid = Grid.clearBoard;
    await this.initWhites();
    await this.initBlacks();
    this.showbrd = true;
  }

  initWhites(){
    this.tablutGrid[this.MIDDLE][this.MIDDLE] = {...WHITE_KING};
    for (let count = 2; count < this.SIZE - 2; count++){
      if (count != this.MIDDLE){
        //mid row
        this.tablutGrid[this.MIDDLE][count] = {...WHITE_PIECE};
        this.tablutGrid[this.MIDDLE][count].rowIndex = this.MIDDLE;
        this.tablutGrid[this.MIDDLE][count].colIndex = count;
        //mid col
        this.tablutGrid[count][this.MIDDLE] = {...WHITE_PIECE};
        this.tablutGrid[count][this.MIDDLE].rowIndex = count;
        this.tablutGrid[count][this.MIDDLE].colIndex = this.MIDDLE;
      }
    }
  }

  initBlacks(){
    for (let count = 3; count < this.SIZE - 3; count++){
      //1st row
      this.tablutGrid[0][count] = {...BLACK_PIECE};
      this.tablutGrid[0][count].rowIndex = 0;
      this.tablutGrid[0][count].colIndex = count;
      //lst row
      this.tablutGrid[this.SIZE-1][count] = {...BLACK_PIECE};
      this.tablutGrid[this.SIZE-1][count].rowIndex = this.SIZE-1;
      this.tablutGrid[this.SIZE-1][count].colIndex = count;
      //1st col
      this.tablutGrid[count][0] = {...BLACK_PIECE};
      this.tablutGrid[count][0].rowIndex = count;
      this.tablutGrid[count][0].colIndex = 0;
      //lst col
      this.tablutGrid[count][this.SIZE-1] = {...BLACK_PIECE};
      this.tablutGrid[count][this.SIZE-1].rowIndex = count;
      this.tablutGrid[count][this.SIZE-1].colIndex = this.SIZE-1;
    }
    this.tablutGrid[1][this.MIDDLE] = {...BLACK_PIECE};
    this.tablutGrid[1][this.MIDDLE].rowIndex = 1;
    this.tablutGrid[1][this.MIDDLE].colIndex = this.MIDDLE;

    this.tablutGrid[this.SIZE-2][this.MIDDLE] = {...BLACK_PIECE};
    this.tablutGrid[this.SIZE-2][this.MIDDLE].rowIndex = this.SIZE-2;
    this.tablutGrid[this.SIZE-2][this.MIDDLE].colIndex = this.MIDDLE;

    this.tablutGrid[this.MIDDLE][1] = {...BLACK_PIECE};
    this.tablutGrid[this.MIDDLE][1].rowIndex = this.MIDDLE;
    this.tablutGrid[this.MIDDLE][1].colIndex = 1;

    this.tablutGrid[this.MIDDLE][this.SIZE-2] = {...BLACK_PIECE};
    this.tablutGrid[this.MIDDLE][this.SIZE-2].rowIndex = this.MIDDLE;
    this.tablutGrid[this.MIDDLE][this.SIZE-2].colIndex = this.SIZE-2;
  }

  restrictMoment(col: PieceInterface | null, r, c) {
    console.log(col, r, c);
    // Blocks entire team based on Turns
    if (!this.selectedPiece) {
      if (col && (col.team === this.turn)) {
        return false;
      }
      return true;
    }
    // Allows to select any Piece only from selected team.
    if (this.selectedPiece && col && (col.team === this.selectedPiece.team)) {
      return false;
    }
    // Blocks all steps except possible paths.
    if (this.col_i !== -1 && this.row_i !== -1) {
      for (let count = 0; count < this.steps.length; count++) {
        if ((r === this.steps[count].r) && (c === this.steps[count].c)) {
          return false;
        }
      }
      return true;
    }
    return false;
  }

  calcHL(r, c) {
    return (r === this.row_i && c === this.col_i);
  }

  checkSteps(r, c) {
    for (let count = 0; count < this.steps.length; count++) {
      if (r === this.steps[count].r && c === this.steps[count].c) { return true; }
    }
    return false;
  }

  // Default Image
  errorHandler(event, r, c) {
    console.log(r, c);
    event.target.src = 'assets/pieces/default.png';
  }

  getImageOnIndex(col, r, c) {
    if (col) {
      return col.img;
    }
    return 'assets/pieces/default.png';
  }

  // On select of Piece
  selectPiece(col: PieceInterface | null, r, c) {
    console.log(this.tablutGrid);
    this.steps = [];
    if (this.col_i !== -1 && this.row_i !== -1) {
      if (col && this.selectedPiece && col.team === this.selectedPiece.team) {
        this.selectedPiece = col; this.col_i = c; this.row_i = r;
        this.checkPieceConditions(col, r, c);
      } else {

        // Checks the winner of the game.
        if (this.tablutGrid[r][c] && this.tablutGrid[r][c].name === 'KING') {
          const winnerTeam = this.selectedPiece.team;
          alert(winnerTeam + ' player wins.');
          return;
        }

        this.tablutGrid[this.row_i][this.col_i] = null;
        this.tablutGrid[r][c] = this.selectedPiece; this.col_i = -1; this.row_i = -1;
        this.steps = [];
        this.turn = this.turn === 'white' ? 'black' : 'white';
        this.selectedPiece = null;

        // // Evolve the soldier at end of the line.
        // if (r === 0 && this.tablutGrid[r][c].name === 'SOLDIER' && this.tablutGrid[r][c].team === 'black') {
        //   this.chooseSoldierEvolution('black', r, c);
        // }
        // if (r === 7 && this.tablutGrid[r][c].name === 'SOLDIER' && this.tablutGrid[r][c].team === 'white') {
        //   this.chooseSoldierEvolution('white', r, c);
        // }
      }
    } else if (col) {
      this.selectedPiece = col; this.col_i = c; this.row_i = r;
      this.checkPieceConditions(col, r, c);
    }
  }

  async checkPieceConditions(col: PieceInterface | null, r, c) {
    console.log(col);
    for (let count = r + 1; count < this.SIZE; count++) { if (await this.pushOnIndividualCondition(col, count, c)) { break; } }
    for (let count = r - 1; count >= 0; count--) { if (await this.pushOnIndividualCondition(col, count, c)) { break; } }
    for (let count = c + 1; count < this.SIZE; count++) { if (await this.pushOnIndividualCondition(col, r, count)) { break; } }
    for (let count = c - 1; count >= 0; count--) { if (await this.pushOnIndividualCondition(col, r, count)) { break; } }
    console.log(this.steps);
  }

  pushOnIndividualCondition(col: PieceInterface | null, r, c) {
    if (!this.tablutGrid[r][c]) {
      this.steps.push({ r, c });
      return false;
    } else {
      // this.breakCondition(col, r, c);
      return true;
    }
  }

  breakCondition(col, r, c) {
    if (this.tablutGrid[r][c] && this.tablutGrid[r][c].team !== col.team) {
      this.steps.push({ r, c });
    }
  }
}

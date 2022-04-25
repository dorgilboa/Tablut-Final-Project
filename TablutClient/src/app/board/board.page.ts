import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Grid } from '../models/grid';
import { BLACK_PIECE, WHITE_KING, WHITE_PIECE } from '../models/pieces';
import { PieceInterface } from '../models/pieces-interface';
import { RestTablutService } from '../rest-tablut.service';

/*
main component - board game inclodes logic of available moves for human player
and the selected moves for both human and AI players
*/
@Component({
  selector: 'app-board',
  templateUrl: './board.page.html',
  styleUrls: ['./board.page.scss'],
})
export class BoardPage implements OnInit {

  gameID;
  aiplayer;
  finished;
  turn = 'white';
  row_i = -1;
  col_i = -1;
  selectedPiece: PieceInterface | null;
  steps = [];
  showbrd = false;
  tablutGrid: any;
  readonly SIZE = 9;
  readonly MIDDLE = Math.floor(this.SIZE/2);

  constructor(route:ActivatedRoute, private rts : RestTablutService, private router: Router) {
      /// set AI color via routing
      let color = router.getCurrentNavigation().finalUrl.queryParamMap.get('compColor')
      console.log(color);
      this.initGame(color);

   }

  ngOnInit() {
  }



  async initGame(color){
    this.finished = false;
    this.initBoard();
    this.rts.sendGetRequest('Game').subscribe((data: any)=>{
      this.gameID = data;
      switch(color){
      case "black":
        this.aiplayer = "black";
        break;
      case "white":
        this.aiplayer = "white";
        this.playAiTurn();
        break;
      default:
        this.aiplayer = "none";
        break;
      }
    });

  }


  async initBoard(){
    /// first player is white - human or AI
    this.turn = 'white';
    this.tablutGrid = Grid.clearBoard;
    await this.clearBoard();
    await this.initWhites();
    await this.initBlacks();
    this.showbrd = true;
  }

  clearBoard(){
    /// clear for games following the first
    for(let i = 0; i < this.SIZE; i++){
      for (let j = 0; j < this.SIZE; j++){
        this.tablutGrid[i][j] = null;
      }
    }
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

  restrictMovement(col: PieceInterface | null, r, c) {
    // Blocks entire grid while waiting for computer's respond.
    if (this.aiplayer === this.turn){
      return true;
    }
    // Blocks entire team based on Turns - first click of game.
    if (!this.selectedPiece) {
      if (col && (col.team === this.turn)) {
        return false;
      }
      return true;
    }
    // Allows to select any Piece only from selected team. - first click in turn.
    if (this.selectedPiece && col && (col.team === this.selectedPiece.team)) {
      return false;
    }
    // Blocks all steps except possible paths. - second click in turn.
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
    /// for highlighting available row and colomn moves
    return (r === this.row_i && c === this.col_i);
  }

  checkSteps(r, c) {
    /// for highlighting available row and colomn moves
    for (let count = 0; count < this.steps.length; count++) {
      if (r === this.steps[count].r && c === this.steps[count].c) { return true; }
    }
    return false;
  }

  // Default Image
  errorHandler(event, r, c) {
    console.log(r, c);
    // empty cell image on err
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
    console.log("selectpiece")
    this.steps = [];
    if (this.col_i !== -1 && this.row_i !== -1) {
      if (col && this.selectedPiece && col.team === this.selectedPiece.team) {
        this.selectedPiece = col; this.col_i = c; this.row_i = r;
        this.checkPieceConditions(col, r, c);
      } else {
        // after moving the piece...
        let move = {
                    from: this.row_i*this.SIZE + this.col_i+1,
                    to: r*this.SIZE+c+1,
                    color: this.turn};
        // takes care of capturing response...
        this.rts.sendPutRequest("Game", this.gameID, move).subscribe((data: any)=>{
          console.log(data);
          this.dataHandler(data);

          this.tablutGrid[this.row_i][this.col_i] = null;
          this.tablutGrid[r][c] = this.selectedPiece; this.col_i = -1; this.row_i = -1;
          this.steps = [];
          this.turn = this.turn === 'white' ? 'black' : 'white';
          this.selectedPiece = null;

          // computer response...
          if (!this.finished && this.aiplayer != "none"){
            this.playAiTurn();
          }
        });
      }
    } else if (col) {
      this.selectedPiece = col; this.col_i = c; this.row_i = r;
      this.checkPieceConditions(col, r, c);
    }
  }

  dataHandler(data:any){
    /// handle respnse from server
    if (typeof data === "string")
      console.log(data);
    else{
      try{
        if (data.length > 0){
          data.forEach(element => {
            if (element == 102){
              this.finished = true;
              alert("White Won.");
              this.router.navigate(['/home']);
            }
            else if (element == 101){
              this.finished = true;
              alert("Black Won.");
              this.router.navigate(['/home']);
            }
            // capturing...
            let row = Math.floor((element -1) / 9);
            let col = element % 9 - 1;
            this.tablutGrid[row][col] = null;
          });
        }
      } catch (error){
        console.error();
      }
    }
  }

  async checkPieceConditions(col: PieceInterface | null, r, c) {
    /// find available steps
    for (let count = r + 1; count < this.SIZE; count++) { if (await this.pushOnIndividualCondition(col, count, c)) { break; } }
    for (let count = r - 1; count >= 0; count--) { if (await this.pushOnIndividualCondition(col, count, c)) { break; } }
    for (let count = c + 1; count < this.SIZE; count++) { if (await this.pushOnIndividualCondition(col, r, count)) { break; } }
    for (let count = c - 1; count >= 0; count--) { if (await this.pushOnIndividualCondition(col, r, count)) { break; } }
  }

  pushOnIndividualCondition(col: PieceInterface | null, r, c) {
    if (!this.tablutGrid[r][c]) {
      this.steps.push({ r, c });
      return false;
    } else {
      return true;
    }
  }

  breakCondition(col, r, c) {
    if (this.tablutGrid[r][c] && this.tablutGrid[r][c].team !== col.team) {
      this.steps.push({ r, c });
    }
  }

  playAiTurn(){
    let data = {
      id : this.gameID,
      color : this.aiplayer
    };
    this.rts.sendPostRequest("Game", data).subscribe((respond: any)=>{
      console.log(respond);
      this.aiDataHandler(respond);
    });
  }


  aiDataHandler(data:any){
    let rowf = Math.floor((data[0] - 1) / 9);
    let colf = data[0] % 9 - 1;
    let rowt = Math.floor((data[1] - 1) / 9);
    let colt = data[1] % 9 - 1;
    // moving...
    this.tablutGrid[rowt][colt] = this.tablutGrid[rowf][colf];
    this.tablutGrid[rowf][colf] = null;
    // capturing...
    this.dataHandler(data.slice(2));

    this.turn = this.turn === 'white' ? 'black' : 'white';
  }


}


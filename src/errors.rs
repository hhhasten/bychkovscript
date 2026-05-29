pub struct LexerError;

impl LexerError {
    pub fn unknown_char(c: char, line: usize, col: usize) -> String {
        format!("[BychkovScript] unknown symbol '{}' ({}:{})", c, line, col)
    }

    pub fn double_eq(line: usize, col: usize) -> String {
        format!(
            "[BychkovScript] use '===' instead of '==' ({}:{})",
            line, col
        )
    }

    pub fn double_not_eq(line: usize, col: usize) -> String {
        format!(
            "[BychkovScript] use '!==' instead of '!=' ({}:{})",
            line, col
        )
    }

    pub fn unterminated_string(line: usize, col: usize) -> String {
        format!("[BychkovScript] Unclosed bracket ({}:{})", line, col)
    }
}

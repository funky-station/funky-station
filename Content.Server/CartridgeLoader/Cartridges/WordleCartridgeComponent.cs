namespace Content.Server.CartridgeLoader.Cartridges;

[RegisterComponent]
public sealed partial class WordleCartridgeComponent : Component
{
    public const string ProgramName = "wordle-program-name";
    /// <summary>
    /// The secret word to guess (5 letters)
    /// </summary>
    [DataField("secretWord")]
    public string SecretWord = "WORDLE";

    /// <summary>
    /// The current guess being built
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public string CurrentGuess = "";

    /// <summary>
    /// List of previous guesses (each 5 letters)
    /// </summary>
    [DataField("previousGuesses")]
    public List<string> PreviousGuesses = new();

    /// <summary>
    /// Letter states for each guess: 0 = not guessed, 1 = wrong spot, 2 = correct spot, 3 = not in word
    /// </summary>
    [ViewVariables]
    public List<List<int>> LetterStates = new();

    /// <summary>
    /// Number of guesses remaining
    /// </summary>
    [DataField("attempts")]
    public int AttemptsRemaining = 6;

    /// <summary>
    /// Has the player won the game?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool GameWon = false;

    /// <summary>
    /// Has the player lost the game?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool GameLost = false;

    // List of valid 5-letter words for the game
    public static readonly List<string> ValidWords = new()
    {
        "ABOUT", "ABOVE", "ABUSE", "ADMIT", "ADOPT", "ADULT", "AFTER", "AGAIN", "AGENT", "AGREE",
        "AHEAD", "ALARM", "ALBUM", "ALERT", "ALGAE", "ALIKE", "ALIGN", "ALIVE", "ALLOW", "ALONE",
        "ALONG", "ALTER", "AMBER", "AMEND", "ANGEL", "ANGER", "ANGLE", "ANGRY", "ANKLE", "ANNOY",
        "APART", "APPLE", "APPLY", "ARENA", "ARGUE", "ARISE", "ARMOR", "AROMA", "AROSE", "ARRAY",
        "ARROW", "ARSON", "ATLAS", "ATOLL", "ATONE", "AUDIO", "AUDIT", "AVOID", "AWAIT", "AWAKE",
        "AWARD", "AWARE", "AWFUL", "AXIAL", "AXIOM", "AZURE", "BACON", "BADGE", "BADLY", "BAGEL",
        "BAKER", "BALKS", "BALLS", "BALMY", "BANAL", "BANDS", "BANDY", "BANGS", "BANJO", "BANKS",
        "BARON", "BASED", "BASIC", "BASIN", "BATCH", "BATHE", "BATON", "BEACH", "BEADS", "BEAMS",
        "BEANS", "BEARD", "BEARS", "BEAST", "BEATS", "BELLE", "BELLS", "BELLY", "BELOW", "BELTS",
        "BENCH", "BERTH", "BETTY", "BIKES", "BILLS", "BILLY", "BIRCH", "BIRDS", "BIRTH", "BLACK",
        "BLADE", "BLAME", "BLANK", "BLAST", "BLEAK", "BLEAT", "BLEED", "BLEND", "BLESS", "BLIND",
        "BLINK", "BLISS", "BLOCK", "BLOOD", "BLOOM", "BLOWN", "BLUFF", "BLUNT", "BLURB", "BLURS",
        "BLUSH", "BOARD", "BOAST", "BOATS", "BOBBY", "BOGEY", "BOGUS", "BOILS", "BOLTS", "BOMBS",
        "BONDS", "BONED", "BONES", "BONUS", "BOOKS", "BOOMS", "BOOST", "BOOTH", "BOOTS", "BOOZE",
        "BOOZY", "BORDER", "BORED", "BORER", "BOUND", "BOWEL", "BOWLS", "BOXER", "BOXES", "BRACED",
        "BRACE", "BRADS", "BRAGS", "BRAID", "BRAIN", "BRAKE", "BRAND", "BRASS", "BRAVE", "BRAVO",
        "BRAWL", "BRAWN", "BREAD", "BREAK", "BREED", "BREWS", "BRICK", "BRIDE", "BRIEF", "BRINE",
        "BRING", "BRINK", "BRINY", "BRISK", "BROAD", "BROKE", "BROOD", "BROOK", "BROOM", "BROTH",
        "BROWN", "BROWS", "BRUCE", "BRUIN", "BRUNT", "BRUSH", "BRUTE", "BUDDY", "BUDGE", "BUFFS",
        "BUILD", "BUILT", "BULGE", "BULKS", "BULKY", "BULLS", "BULLY", "BUMPS", "BUMPY", "BUNCH",
        "BUNKS", "BUNNY", "BUOYS", "BURGS", "BURLY", "BURNS", "BURNT", "BURPS", "BURRO", "BURRS",
        "BURST", "BUSES", "BUSHY", "BUSTS", "BUTCH", "BUTTE", "BUTTS", "BUYER", "BUZZY", "CABAL",
        "CABIN", "CABLE", "CACAO", "CACHE", "CACTI", "CAGES", "CAGEY", "CAKES", "CAKEY", "CALIF",
        "CALLS", "CALMS", "CALVE", "CAMEL", "CAMEO", "CAMPS", "CANAL", "CANDY", "CANED", "CANES",
        "CANOE", "CANON", "CAPER", "CAPES", "CARDS", "CARED", "CARER", "CARES", "CARGO", "CAROL",
        "CAROM", "CARPS", "CARRY", "CARTS", "CARVE", "CASES", "CASKS", "CASTE", "CATCH", "CATER",
        "CAUSE", "CAVES", "CEASE", "CEDAR", "CEDED", "CELLS", "CENTS", "CHAIN", "CHAIR", "CHALK",
        "CHAMP", "CHANT", "CHAOS", "CHAPS", "CHARD", "CHARM", "CHARS", "CHART", "CHASE", "CHASM",
        "CHATS", "CHEAP", "CHEAT", "CHECK", "CHEEK", "CHEER", "CHESS", "CHEST", "CHEWS", "CHEWY",
        "CHICK", "CHICO", "CHIDE", "CHIEF", "CHILD", "CHILL", "CHIME", "CHIMP", "CHINA", "CHINK",
        "CHIPS", "CHIRP", "CHOSE", "CHOPS", "CHUBS", "CHUCK", "CHUMP", "CHUNK", "CHURN", "CHUTE",
        "CIDER", "CIGAR", "CINCH", "CIRCA", "CITED", "CITES", "CIVIC", "CIVIL", "CLAIM", "CLAMP",
        "CLAMS", "CLANG", "CLANK", "CLAPS", "CLASH", "CLASP", "CLASS", "CLAWS", "CLAYS", "CLEAN",
        "CLEAR", "CLEAT", "CLEFT", "CLERK", "CLICK", "CLIFF", "CLIMB", "CLING", "CLOAK", "CLOCK",
        "CLODS", "CLOGS", "CLONE", "CLOSE", "CLOTH", "CLOUD", "CLOUT", "CLOVE", "CLOWN", "CLUBS",
        "CLUCK", "CLUED", "CLUES", "CLUMP", "CLUNG", "COACH", "COALS", "COAST", "COATS", "COBRA",
        "COCKS", "COCKY", "COCOA", "CODES", "COILS", "COINS", "COKED", "COKES", "COLDS", "COLON",
        "COLOR", "COLTS", "COMBS", "COMER", "COMES", "COMET", "COMIC", "COMMA", "CONDO", "CONED",
        "CONES", "CONEY", "CONGO", "CONKS", "CORAL", "CORDS", "CORES", "CORKS", "CORKY", "CORNS",
        "CORNY", "CORPS", "COSTS", "COUCH", "COUGH", "COULD", "COUNT", "COUPE", "COURT", "COUTH",
        "COVED", "COVEN", "COVER", "COVES", "COVET", "COWED", "COWER", "COYLY", "CRABS", "CRACK",
        "CRAFT", "CRAGS", "CRAMP", "CRANE", "CRANK", "CRAPE", "CRAPS", "CRASH", "CRASS", "CRATE",
        "CRAVE", "CRAWL", "CRAWS", "CRAZE", "CRAZY", "CREAK", "CREAM", "CREED", "CREEK", "CREEP",
        "CREPE", "CREPT", "CRESS", "CREST", "CREWS", "CRIBS", "CRIED", "CRIER", "CRIES", "CRIME",
        "CRIMP", "CROAK", "CROCK", "CRONE", "CRONY", "CROOK", "CROON", "CROPS", "CROSS", "CROUP",
        "CROWD", "CROWN", "CROWS", "CRUDE", "CRUEL", "CRUET", "CRUMB", "CRUSH", "CRUST", "CRYPT",
        "CUBED", "CUBES", "CUBIC", "CUBIT", "CUDDY", "CUFFS", "CULLS", "CULMS", "CUMIN", "CUPID",
        "CURBS", "CURDS", "CURLY", "CURRY", "CURSE", "CURVE", "CURVY", "CUSHY", "CUSPS", "CUTCH",
        "CUTER", "CUTES", "CUTEY", "CUTIS", "CUTUP", "CYCLE", "CYDER", "CYNIC", "CYSTS", "CZARS",
        "DADDY", "DAILY", "DAIRY", "DAISY", "DALLY", "DAMES", "DAMPS", "DANCE", "DANDY", "DARED",
        "DARER", "DARES", "DARKS", "DARNS", "DARTS", "DAUBS", "DAUNT", "DAWNS", "DAZED", "DAZES",
        "DEACON", "DEALS", "DEALT", "DEANS", "DEARS", "DEATH", "DEBAE", "DEBAR", "DEBIT", "DEBTS",
        "DEBUG", "DEBUT", "DECAF", "DECAL", "DECAY", "DECKS", "DECOR", "DECOY", "DECRY", "DEEDS",
        "DEEMS", "DEEPS", "DEERS", "DEFER", "DEFY", "DEIST", "DEIFY", "DEITY", "DELAY", "DELFT",
        "DELTA", "DELVE", "DEMON", "DEMUR", "DENIM", "DENSE", "DENTS", "DEITY", "DEOXY", "DEPART",
        "DEPTH", "DERBY", "DERIK", "DESKS", "DESPAIR", "DETER", "DETOX", "DEUCE", "DEVIL", "DEVISE",
        "DEVOID", "DEVOTE", "DEWED", "DEWY", "DESKS", "DEWS", "DEWAX", "DEZZY", "DHARMA", "DHOTI",
        "DIADS", "DIARY", "DICED", "DICER", "DICES", "DICKY", "DICOT", "DIDOS", "DIETS", "DIETY",
        "DIFFS", "DIGIT", "DIKED", "DIKES", "DIMLY", "DIMER", "DIMLY", "DIMMER", "DIMPLE", "DINAR",
        "DINGY", "DINED", "DINER", "DINES", "DINGO", "DINGS", "DINGY", "DINGY", "DIODE", "DIOLS",
        "DIOXS", "DIOXY", "DIPPED", "DIPPER", "DIPSO", "DIPSY", "DIPTYCH", "DIRGE", "DIRKS", "DIRTY",
        "DISCO", "DISCS", "DISHY", "DISME", "DISMS", "DISTES", "DITCH", "DITER", "DITES", "DITHER",
        "DITSY", "DITTO", "DITTY", "DIVAS", "DIVED", "DIVEL", "DIVER", "DIVES", "DIVOT", "DIVVY",
        "DJINN", "DJINS", "DOCKS", "DODOS", "DOERS", "DOFFS", "DOGES", "DOGGY", "DOGMA", "DOING",
        "DOITS", "DOKED", "DOKEY", "DOLES", "DOLLS", "DOLLY", "DOLOR", "DOLTS", "DOMED", "DOMES",
        "DONAH", "DONAH", "DONAH", "DONAS", "DONEE", "DONER", "DONES", "DONGA", "DONGO", "DONGS",
        "DONNA", "DONNE", "DONOR", "DONOS", "DONSY", "DONUT", "DONYA", "DONZE", "DOOBY", "EBOOK",
        "DOOCE", "DOOCY", "DOOED", "DOOEY", "DOOFY", "DOOFY", "DOOMY", "DOOMS", "DOOMY", "DOPEE",
        "DOPER", "DOPES", "DOPEY", "DOPEY", "DOPING", "DOPED", "DOPIES", "DOPIEST", "DOPPEL", "DOPPLER",
        "DORKS", "DORKY", "DORMS", "DOROB", "DOROS", "DORSA", "DORSO", "DORSY", "DORTS", "DORTY",
        "DOSED", "DOSER", "DOSES", "DOTAL", "DOTED", "DOTER", "DOTES", "DOTEY", "DOTING", "DOTOA",
        "DOTOH", "DOTOM", "DOTON", "DOTOS", "DOTSY", "DOTTY", "DOUAN", "DOUAY", "DOUAY", "DOUBT",
        "DOUBY", "DOUCH", "DOUCE", "DOUGH", "DOUGH", "DOUMA", "DOUME", "DOUMS", "DOURA", "DOURS",
        "DOUSE", "DOUST", "DOUTH", "DOUTY", "DOVE", "DOVED", "DOVEN", "DOVER", "DOVES", "DOVEY",
        "DOVET", "DOVEY", "DOVIE", "DOVIE", "DOVIE", "DOVIN", "DOVIN", "DOVUN", "DOVVY", "DOWAH",
        "DOWAK", "DOWAK", "DOWEL", "DOWER", "DOWFA", "DOWNBY", "DOWNE", "DOWNER", "DOWNES", "DOWNEY",
        "DOWNY", "DOWNZ", "DOWNY", "DOWRF", "DOWRG", "DOWRH", "DOWRI", "DOWRJ", "DOWRK", "DOWRL",
        "DOWRM", "DOWRN", "DOWRO", "DOWRP", "DOWRQ", "DOWRR", "DOWRS", "DOWRT", "DOWRU", "DOWRV",
        "DOWRW", "DOWRX", "DOWRY", "DOWRZ", "DOWSA", "DOWSB", "DOWSC", "DOWSD", "DOWSE", "DOWSF",
        "DOWSG", "DOWSH", "DOWSI", "DOWSJ", "DOWSK", "DOWSL", "DOWSM", "DOWSN", "DOWSO", "DOWSP",
        "DOWSQ", "DOWSR", "DOWSS", "DOWST", "DOWSU", "DOWSV", "DOWSW", "DOWSX", "DOWSY", "DOWSZ",
        "DOWTA", "DOWTB", "DOWTC", "DOWTD", "DOWTE", "DOWTF", "DOWTG", "DOWTH", "DOWTI", "DOWTJ",
        "DOWTK", "DOWTL", "DOWTM", "DOWTN", "DOWTO", "DOWTP", "DOWTQ", "DOWTR", "DOWTS", "DOWTT",
        "DOWTU", "DOWTV", "DOWTW", "DOWTX", "DOWTY", "DOWTZ", "DOWUA", "DOWUB", "DOWUC", "DOWUD",
        "DOWUE", "DOWUF", "DOWUG", "DOWUH", "DOWUI", "DOWUJ", "DOWUK", "DOWUL", "DOWUM", "DOWUN",
        "DOWUO", "DOWUP", "DOWUQ", "DOWUR", "DOWUS", "DOWUT", "DOWUU", "DOWUV", "DOWUW", "DOWUX",
        "DOWUY", "DOWUZ", "DOWVA", "DOWVB", "DOWVC", "DOWVD", "DOWVE", "DOWVF", "DOWVG", "DOWVH"
    };

    public static string GetRandomWord()
    {
        var random = new Random();
        return ValidWords[random.Next(ValidWords.Count)];
    }
}
